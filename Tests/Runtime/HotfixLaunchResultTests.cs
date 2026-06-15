using GameFrameX.Startup.Runtime;

using NUnit.Framework;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class HotfixLaunchResultTests
    {
        [Test]
        public void Succeed_ReturnsSuccessWithEmptyError()
        {
            var result = HotfixLaunchResult.Succeed();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        [Test]
        public void Fail_ReturnsFailureWithErrorMessage()
        {
            var result = HotfixLaunchResult.Fail("DLL not found");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("DLL not found", result.ErrorMessage);
        }

        [Test]
        public void Fail_WithNullError_TreatsAsEmptyString()
        {
            var result = HotfixLaunchResult.Fail(null);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        [Test]
        public void IsClass()
        {
            Assert.IsFalse(typeof(HotfixLaunchResult).IsValueType, "HotfixLaunchResult should be a class");
        }
    }
}
