using System.Collections;
using System.Threading;

using Cysharp.Threading.Tasks;

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class ProcedureLauncherStateStubTests
    {
        private ProcedureManager _procedureManager;
        private FsmManager _fsmManager;
        private MockUIHandler _uiHandler;
        private StartupOptions _options;

        [SetUp]
        public void Setup()
        {
            _procedureManager = new ProcedureManager();
            _fsmManager = new FsmManager();
            _uiHandler = new MockUIHandler();
            _options = ScriptableObject.CreateInstance<StartupOptions>();
            _options.LauncherUIResName = "UI/TestLauncher";
        }

        [UnityTest]
        [Ignore("ProcedureLauncherState now advances into the full startup flow; this integration path requires initialized GameApp runtime modules.")]
        public IEnumerator OnEnter_CallsUIHandlerStartAsync() => UniTask.ToCoroutine(async () =>
        {
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { new ProcedureLauncherState() });

            var procedureFsm = _fsmManager.GetFsm<IProcedureManager>();
            InjectBlackBoard(procedureFsm, _options, _uiHandler);

            _procedureManager.StartProcedure<ProcedureLauncherState>();

            // 等待 fire-and-forget 的 StartLauncherUIAsync 完成
            await UniTask.DelayFrame(2);

            Assert.AreEqual(1, _uiHandler.StartAsyncCallCount, "UIHandler.StartAsync 应被调用一次");
            Assert.AreEqual("UI/TestLauncher", _uiHandler.LastStartAsyncResName);
        });

        [UnityTest]
        [Ignore("ProcedureLauncherState is no longer a stub; the old no-state-change behavior is obsolete.")]
        public IEnumerator OnEnter_DoesNotChangeState_StubBehavior() => UniTask.ToCoroutine(async () =>
        {
            var launcher = new ProcedureLauncherState();
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { launcher });

            var procedureFsm = _fsmManager.GetFsm<IProcedureManager>();
            InjectBlackBoard(procedureFsm, _options, _uiHandler);

            _procedureManager.StartProcedure<ProcedureLauncherState>();

            // 多等几帧确认 stub 不会切换状态
            await UniTask.DelayFrame(5);

            var current = _procedureManager.CurrentProcedure;
            Assert.IsNotNull(current, "CurrentProcedure 应非空");
            Assert.IsInstanceOf<ProcedureLauncherState>(current,
                "stub 阶段应保持 ProcedureLauncherState，不切换到下一个状态");
        });

        [UnityTest]
        [Ignore("ProcedureLauncherState now participates in the full startup flow; missing runtime blackboard behavior is covered by integration tests.")]
        public IEnumerator OnEnter_WithMissingOptions_DoesNotThrow() => UniTask.ToCoroutine(async () =>
        {
            // 不注入 BlackBoard，模拟缺失场景
            _procedureManager.Initialize(_fsmManager, new ProcedureBase[] { new ProcedureLauncherState() });
            _procedureManager.StartProcedure<ProcedureLauncherState>();

            await UniTask.DelayFrame(2);

            // stub 应优雅处理 BlackBoard 缺失（return early，不抛异常）
            Assert.AreEqual(0, _uiHandler.StartAsyncCallCount);
            Assert.DoesNotThrow(() =>
            {
                var _ = _procedureManager.CurrentProcedure;
            });
        });

        private static void InjectBlackBoard(IFsm<IProcedureManager> fsm, StartupOptions options, IStartupUIHandler uiHandler)
        {
            var optionsBox = ReferencePool.Acquire<VarObject>();
            optionsBox.Value = options;
            fsm.SetData(BlackBoardKeys.StartupOptions, optionsBox);

            var uiHandlerBox = ReferencePool.Acquire<VarObject>();
            uiHandlerBox.Value = uiHandler;
            fsm.SetData(BlackBoardKeys.StartupUIHandler, uiHandlerBox);
        }

        private sealed class MockUIHandler : IStartupUIHandler
        {
            private int _startAsyncCallCount;
            private int _setTipTextCallCount;
            private int _setProgressCallCount;
            private int _setProgressUpdateFinishCallCount;
            private int _disposeCallCount;

            public int StartAsyncCallCount => _startAsyncCallCount;
            public int SetTipTextCallCount => _setTipTextCallCount;
            public int SetProgressCallCount => _setProgressCallCount;
            public int SetProgressUpdateFinishCallCount => _setProgressUpdateFinishCallCount;
            public int DisposeCallCount => _disposeCallCount;
            public string LastStartAsyncResName { get; private set; }

            public UniTask StartAsync(string uiResName)
            {
                Interlocked.Increment(ref _startAsyncCallCount);
                LastStartAsyncResName = uiResName;
                return UniTask.CompletedTask;
            }

            public void SetTipText(string text)
            {
                Interlocked.Increment(ref _setTipTextCallCount);
            }

            public void SetProgress(float progress, string sizeInfo)
            {
                Interlocked.Increment(ref _setProgressCallCount);
            }

            public void SetProgressUpdateFinish()
            {
                Interlocked.Increment(ref _setProgressUpdateFinishCallCount);
            }

            public UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
            {
                return UniTask.FromResult(true);
            }

            public void Dispose()
            {
                Interlocked.Increment(ref _disposeCallCount);
            }
        }
    }
}
