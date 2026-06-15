using System.Reflection;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupOptionsTests
    {
        [Test]
        public void DefaultValues_AreCorrect()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();

            // Network
            Assert.IsNotNull(options.GlobalInfoUrls);
            Assert.AreEqual(0, options.GlobalInfoUrls.Length, "GlobalInfoUrls default should be empty array");
            Assert.AreEqual(3, options.MaxAttemptsPerUrl);
            Assert.AreEqual(3000, options.RetryDelayMs);

            // Hotfix
            Assert.AreEqual("Unity.Hotfix", options.HotfixAssemblyName);
            Assert.AreEqual("Hotfix.HotfixLauncher", options.HotfixEntryTypeName);
            Assert.AreEqual("Main", options.HotfixEntryMethodName);

            // HTTP Base Params
            Assert.AreEqual(string.Empty, options.PackageName);
            Assert.AreEqual(string.Empty, options.Channel);
            Assert.AreEqual(string.Empty, options.SubChannel);

            // UI
            Assert.AreEqual("UI/UILauncher", options.LauncherUIResName);
        }

        [Test]
        public void IsScriptableObject()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            Assert.IsNotNull(options);
            Assert.IsInstanceOf<ScriptableObject>(options);
        }

        [Test]
        public void HasCreateAssetMenuAttribute()
        {
            var attributes = typeof(StartupOptions).GetCustomAttributes(typeof(CreateAssetMenuAttribute), false);
            Assert.AreEqual(1, attributes.Length, "StartupOptions should have [CreateAssetMenu]");

            var menuAttr = (CreateAssetMenuAttribute)attributes[0];
            Assert.AreEqual("GameFrameX/Startup Options", menuAttr.menuName);
        }

        [Test]
        public void HasTenPublicFields()
        {
            var publicFields = typeof(StartupOptions).GetFields(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(10, publicFields.Length,
                "StartupOptions should expose exactly 10 public fields per spec §3.1.1");
        }
    }
}
