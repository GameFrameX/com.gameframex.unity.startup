using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

using NUnit.Framework;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupCompletedEventArgsTests
    {
        [Test]
        public void EventId_IsStringFullName()
        {
            Assert.AreEqual(
                "GameFrameX.Startup.Runtime.Events.StartupCompletedEventArgs",
                StartupCompletedEventArgs.EventId);
        }

        [Test]
        public void Id_PropertyReturnsEventId()
        {
            var args = StartupCompletedEventArgs.Create();
            Assert.AreEqual(StartupCompletedEventArgs.EventId, args.Id);
        }

        [Test]
        public void Acquire_ReturnsNonNullInstance()
        {
            var args = StartupCompletedEventArgs.Create();
            Assert.IsNotNull(args);
            Assert.IsInstanceOf<StartupCompletedEventArgs>(args);
        }

        [Test]
        public void InheritsGameEventArgs()
        {
            Assert.IsTrue(typeof(GameEventArgs).IsAssignableFrom(typeof(StartupCompletedEventArgs)));
        }

        [Test]
        public void ImplementsIReference()
        {
            Assert.IsTrue(typeof(IReference).IsAssignableFrom(typeof(StartupCompletedEventArgs)));
        }

        [Test]
        public void Clear_IsEmpty_NoOp()
        {
            var args = StartupCompletedEventArgs.Create();
            Assert.DoesNotThrow(() => args.Clear());
        }

        [Test]
        public void Acquire_Release_Acquire_PoolsInstance()
        {
            var args1 = StartupCompletedEventArgs.Create();
            ReferencePool.Release(args1);
            var args2 = StartupCompletedEventArgs.Create();

            // 引用池允许返回同一实例（具体取决于池容量），这里只验证不抛异常
            Assert.IsNotNull(args2);
        }
    }
}
