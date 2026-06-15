using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

using NUnit.Framework;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupFailedEventArgsTests
    {
        [Test]
        public void EventId_IsStringFullName()
        {
            Assert.AreEqual(
                "GameFrameX.Startup.Runtime.Events.StartupFailedEventArgs",
                StartupFailedEventArgs.EventId);
        }

        [Test]
        public void Id_PropertyReturnsEventId()
        {
            var args = StartupFailedEventArgs.Create("ProcedureX", "http://example.com", "boom");
            Assert.AreEqual(StartupFailedEventArgs.EventId, args.Id);
        }

        [Test]
        public void Create_PopulatesFields()
        {
            var args = StartupFailedEventArgs.Create("ProcedureLauncher", "http://x/api", "timeout");

            Assert.AreEqual("ProcedureLauncher", args.FailedProcedureName);
            Assert.AreEqual("http://x/api", args.FailedUrl);
            Assert.AreEqual("timeout", args.ErrorMessage);
        }

        [Test]
        public void Create_WithNullArgs_TreatsAsEmptyStrings()
        {
            var args = StartupFailedEventArgs.Create(null, null, null);

            Assert.AreEqual(string.Empty, args.FailedProcedureName);
            Assert.AreEqual(string.Empty, args.FailedUrl);
            Assert.AreEqual(string.Empty, args.ErrorMessage);
        }

        [Test]
        public void Clear_ResetsAllFields()
        {
            var args = StartupFailedEventArgs.Create("ProcedureX", "http://x/api", "boom");

            args.Clear();

            Assert.AreEqual(string.Empty, args.FailedProcedureName);
            Assert.AreEqual(string.Empty, args.FailedUrl);
            Assert.AreEqual(string.Empty, args.ErrorMessage);
        }

        [Test]
        public void InheritsGameEventArgs()
        {
            Assert.IsTrue(typeof(GameEventArgs).IsAssignableFrom(typeof(StartupFailedEventArgs)));
        }

        [Test]
        public void ImplementsIReference()
        {
            Assert.IsTrue(typeof(IReference).IsAssignableFrom(typeof(StartupFailedEventArgs)));
        }
    }
}
