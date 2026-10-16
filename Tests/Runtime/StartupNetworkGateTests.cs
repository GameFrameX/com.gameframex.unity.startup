using System.Collections;

using Cysharp.Threading.Tasks;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupNetworkGateTests
    {
        [UnityTest]
        public IEnumerator WaitForIOSNetworkAsync_NullOptions_CompletesImmediately() => UniTask.ToCoroutine(async () =>
        {
            var uiHandler = new MockUIHandler();

            await StartupNetworkGate.WaitForIOSNetworkAsync(null, uiHandler);

            Assert.AreEqual(0, uiHandler.SetTipTextCallCount);
        });

        [UnityTest]
        public IEnumerator WaitForIOSNetworkAsync_EmptyProbeUrl_CompletesImmediately() => UniTask.ToCoroutine(async () =>
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            options.IOSNetworkProbeUrl = string.Empty;
            var uiHandler = new MockUIHandler();

            await StartupNetworkGate.WaitForIOSNetworkAsync(options, uiHandler);

            Assert.AreEqual(0, uiHandler.SetTipTextCallCount);
        });

        private sealed class MockUIHandler : IStartupUIHandler
        {
            public int SetTipTextCallCount { get; private set; }

            public UniTask StartAsync(string uiResName)
            {
                return UniTask.CompletedTask;
            }

            public void SetTipText(string text)
            {
                SetTipTextCallCount++;
            }

            public void SetProgress(float progress, string sizeInfo) { }

            public void SetProgressUpdateFinish() { }

            public UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
            {
                return UniTask.FromResult(true);
            }

            public void Dispose() { }
        }
    }
}
