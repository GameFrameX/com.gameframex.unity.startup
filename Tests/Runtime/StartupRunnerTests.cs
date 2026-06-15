using System;
using System.Collections;

using Cysharp.Threading.Tasks;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupRunnerTests
    {
        [Test]
        public void Run_NullOptions_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                StartupRunner.Run(null, new MockUIHandler(), new MockHotfixLauncher());
            });

            Assert.AreEqual("options", ex.ParamName);
        }

        [Test]
        public void Run_NullUiHandler_ThrowsArgumentNullException()
        {
            var options = CreateValidOptions();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                StartupRunner.Run(options, null, new MockHotfixLauncher());
            });

            Assert.AreEqual("uiHandler", ex.ParamName);
        }

        [Test]
        public void Run_NullHotfixLauncher_ThrowsArgumentNullException()
        {
            var options = CreateValidOptions();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                StartupRunner.Run(options, new MockUIHandler(), null);
            });

            Assert.AreEqual("hotfixLauncher", ex.ParamName);
        }

        [Test]
        public void Run_EmptyGlobalInfoUrls_ThrowsArgumentExceptionSynchronously()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            options.GlobalInfoUrls = new string[0];

            // 同步抛出：不进入 async 状态机，await 之前即可捕获
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                var _ = StartupRunner.Run(options, new MockUIHandler(), new MockHotfixLauncher());
            });

            Assert.IsNotNull(ex);
            StringAssert.Contains("GlobalInfoUrls", ex.Message,
                "异常消息应包含 'GlobalInfoUrls' 字段名（spec AC-2）");
            StringAssert.Contains("GlobalInfoUrls", ex.ParamName ?? string.Empty,
                "ArgumentException.ParamName 应为 'GlobalInfoUrls'");
        }

        [Test]
        public void Run_NullGlobalInfoUrls_ThrowsArgumentExceptionSynchronously()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            options.GlobalInfoUrls = null;

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                var _ = StartupRunner.Run(options, new MockUIHandler(), new MockHotfixLauncher());
            });

            StringAssert.Contains("GlobalInfoUrls", ex.Message);
        }

        [UnityTest]
        [Ignore("Full StartupRunner flow requires initialized GameApp runtime modules and is covered as an integration path.")]
        public IEnumerator Run_ValidInputs_InjectsBlackBoardEntries() => UniTask.ToCoroutine(async () =>
        {
            var options = CreateValidOptions();
            var uiHandler = new MockUIHandler();
            var hotfixLauncher = new MockHotfixLauncher();

            // Run 返回 UniTask，但 stub 阶段不会 complete（bootstrap-1 行为）
            UniTask<StartupResult> task = StartupRunner.Run(options, uiHandler, hotfixLauncher);

            // 等待一帧让 ProcedureLauncherState.OnEnter 的 fire-and-forget 跑完
            await UniTask.DelayFrame(3);

            // AC-14 验证：通过 UIHandler 被调用反推 BlackBoard 注入成功
            Assert.AreEqual(1, uiHandler.StartAsyncCallCount,
                "UIHandler.StartAsync 应被调用一次，说明 BlackBoard 注入成功（AC-14）");
            Assert.AreEqual(options.LauncherUIResName, uiHandler.LastStartAsyncResName,
                "StartAsync 接收的 uiResName 应等于 options.LauncherUIResName");

            // stub 阶段 tcs.Task 不会 complete，task 应仍处于等待状态
            Assert.IsFalse(task.GetAwaiter().IsCompleted,
                "bootstrap-1 stub 阶段 UniTask 不应 complete（Procedure 状态机不前进）");
        });

        [UnityTest]
        [Ignore("Full StartupRunner flow requires initialized GameApp runtime modules and is covered as an integration path.")]
        public IEnumerator Run_ValidInputs_ActivatesProcedureLauncherState() => UniTask.ToCoroutine(async () =>
        {
            var options = CreateValidOptions();
            var uiHandler = new MockUIHandler();
            var hotfixLauncher = new MockHotfixLauncher();

            var _ = StartupRunner.Run(options, uiHandler, hotfixLauncher);

            await UniTask.DelayFrame(3);

            // AC-1 验证：UIHandler.StartAsync 被调用一次
            Assert.AreEqual(1, uiHandler.StartAsyncCallCount,
                "ProcedureLauncherState 应被激活并调用 UIHandler.StartAsync（AC-1）");
        });

        private static StartupOptions CreateValidOptions()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            options.GlobalInfoUrls = new string[] { "http://example.com/api/globalInfo" };
            options.LauncherUIResName = "UI/TestLauncher";
            return options;
        }

        private sealed class MockUIHandler : IStartupUIHandler
        {
            private int _startAsyncCallCount;
            private int _disposeCallCount;

            public int StartAsyncCallCount => _startAsyncCallCount;
            public int DisposeCallCount => _disposeCallCount;
            public string LastStartAsyncResName { get; private set; }

            public UniTask StartAsync(string uiResName)
            {
                _startAsyncCallCount++;
                LastStartAsyncResName = uiResName;
                return UniTask.CompletedTask;
            }

            public void SetTipText(string text) { }

            public void SetProgress(float progress, string sizeInfo) { }

            public void SetProgressUpdateFinish() { }

            public UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
            {
                return UniTask.FromResult(true);
            }

            public void Dispose()
            {
                _disposeCallCount++;
            }
        }

        private sealed class MockHotfixLauncher : IHotfixLauncher
        {
            public UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
            {
                return UniTask.FromResult(HotfixLaunchResult.Succeed());
            }
        }
    }
}
