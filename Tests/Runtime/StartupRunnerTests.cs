using System;

using Cysharp.Threading.Tasks;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;

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
