using System.Collections;
using System.Threading;

using Cysharp.Threading.Tasks;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class IStartupUIHandlerContractTests
    {
        [Test]
        public void MockImpl_ImplementsAllFiveMethods()
        {
            IStartupUIHandler handler = new MockUIHandler();

            Assert.IsNotNull(handler);
            Assert.DoesNotThrow(() => handler.SetTipText("tip"));
            Assert.DoesNotThrow(() => handler.SetProgress(0.5f, "10MB / 20MB"));
            Assert.DoesNotThrow(() => handler.SetProgressUpdateFinish());
            Assert.DoesNotThrow(() => handler.Dispose());
        }

        [UnityTest]
        public IEnumerator StartAsync_ReturnsCompletedTask() => UniTask.ToCoroutine(async () =>
        {
            IStartupUIHandler handler = new MockUIHandler();
            await handler.StartAsync("UI/UILauncher");
        });

        [Test]
        public void MockImpl_TracksCalls()
        {
            var handler = new MockUIHandler();
            handler.SetTipText("loading");
            handler.SetProgress(0.1f, "1MB");
            handler.SetProgress(0.9f, "9MB");
            handler.SetProgressUpdateFinish();
            handler.Dispose();

            Assert.AreEqual(1, handler.SetTipTextCallCount);
            Assert.AreEqual(2, handler.SetProgressCallCount);
            Assert.AreEqual(1, handler.SetProgressUpdateFinishCallCount);
            Assert.AreEqual(1, handler.DisposeCallCount);
        }

        private sealed class MockUIHandler : IStartupUIHandler
        {
            public int SetTipTextCallCount;
            public int SetProgressCallCount;
            public int SetProgressUpdateFinishCallCount;
            public int DisposeCallCount;

            public UniTask StartAsync(string uiResName)
            {
                return UniTask.CompletedTask;
            }

            public void SetTipText(string text)
            {
                Interlocked.Increment(ref SetTipTextCallCount);
            }

            public void SetProgress(float progress, string sizeInfo)
            {
                Interlocked.Increment(ref SetProgressCallCount);
            }

            public void SetProgressUpdateFinish()
            {
                Interlocked.Increment(ref SetProgressUpdateFinishCallCount);
            }

            public UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
            {
                return UniTask.FromResult(true);
            }

            public void Dispose()
            {
                Interlocked.Increment(ref DisposeCallCount);
            }
        }
    }
}
