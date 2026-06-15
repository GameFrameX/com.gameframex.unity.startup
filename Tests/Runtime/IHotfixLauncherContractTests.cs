using System.Collections;

using Cysharp.Threading.Tasks;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class IHotfixLauncherContractTests
    {
        [UnityTest]
        public IEnumerator MockImpl_ReturnsHotfixLaunchResultSucceed() => UniTask.ToCoroutine(async () =>
        {
            IHotfixLauncher launcher = new SucceedHotfixLauncher();
            var options = ScriptableObject.CreateInstance<StartupOptions>();

            var result = await launcher.StartAsync(options);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        });

        [UnityTest]
        public IEnumerator MockImpl_CanReturnFailure() => UniTask.ToCoroutine(async () =>
        {
            IHotfixLauncher launcher = new FailingHotfixLauncher("DLL not found");
            var options = ScriptableObject.CreateInstance<StartupOptions>();

            var result = await launcher.StartAsync(options);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("DLL not found", result.ErrorMessage);
        });

        private sealed class SucceedHotfixLauncher : IHotfixLauncher
        {
            public UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
            {
                return UniTask.FromResult(HotfixLaunchResult.Succeed());
            }
        }

        private sealed class FailingHotfixLauncher : IHotfixLauncher
        {
            private readonly string _errorMessage;

            public FailingHotfixLauncher(string errorMessage)
            {
                _errorMessage = errorMessage;
            }

            public UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
            {
                return UniTask.FromResult(HotfixLaunchResult.Fail(_errorMessage));
            }
        }
    }
}
