using GameFrameX.Startup.Runtime;

using NUnit.Framework;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupResultTests
    {
        [Test]
        public void Succeed_ReturnsSuccessWithEmptyFields()
        {
            var result = StartupResult.Succeed();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.FailedProcedureName);
            Assert.AreEqual(string.Empty, result.FailedUrl);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        [Test]
        public void Fail_ReturnsFailureWithPopulatedFields()
        {
            var result = StartupResult.Fail("ProcedureX", "http://example.com/api", "boom");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("ProcedureX", result.FailedProcedureName);
            Assert.AreEqual("http://example.com/api", result.FailedUrl);
            Assert.AreEqual("boom", result.ErrorMessage);
        }

        [Test]
        public void Fail_WithNullArgs_TreatsAsEmptyStrings()
        {
            var result = StartupResult.Fail(null, null, null);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(string.Empty, result.FailedProcedureName);
            Assert.AreEqual(string.Empty, result.FailedUrl);
            Assert.AreEqual(string.Empty, result.ErrorMessage);
        }

        [Test]
        public void IsClass()
        {
            Assert.IsFalse(typeof(StartupResult).IsValueType, "StartupResult should be a class");
        }
    }
}
