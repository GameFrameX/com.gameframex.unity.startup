using System.Collections.Generic;
using System.Reflection;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal class StartupOptionsTests
    {
        [Test]
        public void DefaultValues_AreCorrect()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();

            // Asset
            Assert.AreEqual(EPlayMode.EditorSimulateMode, options.GamePlayMode);

            // Network
            Assert.IsNotNull(options.GlobalInfoUrls);
            Assert.AreEqual(0, options.GlobalInfoUrls.Length, "GlobalInfoUrls default should be empty array");
            Assert.AreEqual(string.Empty, options.GameFrameXApiKey);
            Assert.AreEqual(string.Empty, options.GameFrameXAppId);
            Assert.AreEqual(string.Empty, options.GameFrameXAppSecret);
            Assert.AreEqual(3, options.MaxAttemptsPerUrl);
            Assert.AreEqual(3000, options.RetryDelayMs);
            Assert.IsFalse(options.SkipRemoteStartupRequests);

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
        public void HasFifteenPublicFields()
        {
            var publicFields = typeof(StartupOptions).GetFields(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(15, publicFields.Length,
                "StartupOptions should expose exactly 15 public fields per spec §3.1.1");
        }

        [Test]
        public void CreateGameFrameXHeaders_OnlyIncludesNonEmptyValues()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            options.GameFrameXApiKey = "api-key";
            options.GameFrameXAppId = "";
            options.GameFrameXAppSecret = "app-secret";

            var utilityType = typeof(StartupOptions).Assembly.GetType("GameFrameX.Startup.Runtime.StartupProcedureUtility");
            var method = utilityType.GetMethod("CreateGameFrameXHeaders", BindingFlags.Public | BindingFlags.Static);
            var headers = (Dictionary<string, string>)method.Invoke(null, new object[] { options });

            Assert.AreEqual(2, headers.Count);
            Assert.AreEqual("api-key", headers["GameFrameX-Api-Key"]);
            Assert.AreEqual("app-secret", headers["GameFrameX-App-Secret"]);
            Assert.IsFalse(headers.ContainsKey("GameFrameX-App-Id"));
        }
    }
}
