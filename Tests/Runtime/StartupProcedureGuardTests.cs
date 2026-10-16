using System;
using System.Collections;

using Cysharp.Threading.Tasks;

using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    /// <summary>
    /// 验证 Procedure 异常兜底（RunGuardedAsync）、StartupContext 缺失路由与 GameLauncherState 收尾语义。
    /// </summary>
    internal class StartupProcedureGuardTests
    {
        private ProcedureManager _procedureManager;
        private FsmManager _fsmManager;
        private MockUIHandler _uiHandler;
        private MockHotfixLauncher _hotfixLauncher;
        private UniTaskCompletionSource<StartupResult> _completionSource;

        [SetUp]
        public void Setup()
        {
            // EditMode 测试没有场景组件，手动注册模块映射（生产由 GameFrameworkComponent 完成同一注册）
            GameFrameworkEntry.RegisterModule(typeof(IEventManager), typeof(EventManager));

            _procedureManager = new ProcedureManager();
            _fsmManager = new FsmManager();
            _uiHandler = new MockUIHandler();
            _hotfixLauncher = new MockHotfixLauncher();
            _completionSource = new UniTaskCompletionSource<StartupResult>();
        }

        [Test]
        public void RunGuardedAsync_BodyThrows_CompletesFailureInsteadOfHanging()
        {
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { new ThrowingProcedure() });
            var fsm = _fsmManager.GetFsm<IProcedureManager>();
            InjectContext(fsm);
            _procedureManager.StartProcedure<ThrowingProcedure>();

            var awaiter = _completionSource.Task.GetAwaiter();
            Assert.IsTrue(awaiter.IsCompleted, "预期外异常必须被兜底路由为失败结果，awaiter 不允许永挂");
            var result = awaiter.GetResult();
            Assert.IsFalse(result.Success);
            Assert.AreEqual(nameof(ThrowingProcedure), result.FailedProcedureName);
            Assert.That(result.ErrorMessage, Does.Contain("boom"));
            Assert.GreaterOrEqual(_uiHandler.SetTipTextCallCount, 1, "失败信息应展示在启动 UI 上");
            Assert.That(_uiHandler.LastTipText, Does.Contain("boom"));
        }

        [Test]
        public void GetContext_MissingInjection_RoutesDescriptiveFailure()
        {
            // 模拟 Run 已发生（完成源已登记）但 FSM 注入缺失/损坏的场景
            StartupRunner.ActiveCompletionSource = _completionSource;
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { new ProcedureLauncherState() });
            _procedureManager.StartProcedure<ProcedureLauncherState>();

            var awaiter = _completionSource.Task.GetAwaiter();
            Assert.IsTrue(awaiter.IsCompleted, "context 缺失必须路由为失败而不是静默不前进");
            var result = awaiter.GetResult();
            Assert.IsFalse(result.Success);
            Assert.AreEqual(nameof(ProcedureLauncherState), result.FailedProcedureName);
            Assert.That(result.ErrorMessage, Does.Contain("Startup context"));
        }

        [UnityTest]
        public IEnumerator GameLauncherState_HotfixFails_KeepsUIAliveAndShowsError() => UniTask.ToCoroutine(async () =>
        {
            _hotfixLauncher.ResultToReturn = HotfixLaunchResult.Fail("hotfix exploded");
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { new ProcedureGameLauncherState() });
            var fsm = _fsmManager.GetFsm<IProcedureManager>();
            InjectContext(fsm);
            _procedureManager.StartProcedure<ProcedureGameLauncherState>();

            var result = await _completionSource.Task;

            Assert.IsFalse(result.Success);
            Assert.AreEqual(nameof(ProcedureGameLauncherState), result.FailedProcedureName);
            Assert.That(result.ErrorMessage, Does.Contain("hotfix exploded"));
            Assert.AreEqual(0, _uiHandler.DisposeCallCount, "失败时启动 UI 必须保留用于展示错误信息");
            Assert.That(_uiHandler.LastTipText, Does.Contain("hotfix exploded"));
        });

        [UnityTest]
        public IEnumerator GameLauncherState_HotfixSucceeds_DisposesUIAndCompletesSuccess() => UniTask.ToCoroutine(async () =>
        {
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { new ProcedureGameLauncherState() });
            var fsm = _fsmManager.GetFsm<IProcedureManager>();
            InjectContext(fsm);
            _procedureManager.StartProcedure<ProcedureGameLauncherState>();

            var result = await _completionSource.Task;

            Assert.IsTrue(result.Success, "期望成功，实际 Proc=" + result.FailedProcedureName + " Err=" + result.ErrorMessage + " Tip=" + _uiHandler.LastTipText);
            Assert.AreEqual(1, _uiHandler.DisposeCallCount, "成功时启动 UI 应被释放");
        });

        private void InjectContext(IFsm<IProcedureManager> fsm)
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            var context = new StartupContext(options, _uiHandler, _hotfixLauncher, _completionSource, new DefaultStartupHttpParamsProvider());
            var contextBox = ReferencePool.Acquire<VarObject>();
            contextBox.Value = context;
            fsm.SetData(BlackBoardKeys.StartupContext, contextBox);
        }

        /// <summary>
        /// 模拟生产 Procedure 的守卫接线：OnEnter 一律通过 RunGuardedAsync 执行主体。
        /// </summary>
        private sealed class ThrowingProcedure : ProcedureBase
        {
            protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
            {
                base.OnEnter(procedureOwner);
                await StartupProcedureUtility.RunGuardedAsync(procedureOwner, nameof(ThrowingProcedure), ThrowAsync);
            }

            private static UniTask ThrowAsync()
            {
                throw new InvalidOperationException("boom");
            }
        }

        private sealed class MockHotfixLauncher : IHotfixLauncher
        {
            public HotfixLaunchResult ResultToReturn = HotfixLaunchResult.Succeed();

            public UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
            {
                return UniTask.FromResult(ResultToReturn);
            }
        }

        private sealed class MockUIHandler : IStartupUIHandler
        {
            public int SetTipTextCallCount { get; private set; }
            public int DisposeCallCount { get; private set; }
            public string LastTipText { get; private set; }

            public UniTask StartAsync(string uiResName)
            {
                return UniTask.CompletedTask;
            }

            public void SetTipText(string text)
            {
                SetTipTextCallCount++;
                LastTipText = text;
            }

            public void SetProgress(float progress, string sizeInfo)
            {
            }

            public void SetProgressUpdateFinish()
            {
            }

            public UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
            {
                return UniTask.FromResult(true);
            }

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }
    }
}
