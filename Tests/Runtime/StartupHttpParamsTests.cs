using System.IO;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

using UnityEngine;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal sealed class StartupHttpParamsTests
    {
        [Test]
        public void FromOptions_CopiesProjectConfigFields()
        {
            var options = ScriptableObject.CreateInstance<StartupOptions>();
            options.PackageName = "com.company.game";
            options.Channel = "official";
            options.SubChannel = "qa";

            var parameters = StartupHttpParams.FromOptions(options);

            Assert.AreEqual("com.company.game", parameters.PackageName);
            Assert.AreEqual("official", parameters.Channel);
            Assert.AreEqual("qa", parameters.SubChannel);
            Assert.AreEqual(string.Empty, parameters.Language);
            Assert.AreEqual(string.Empty, parameters.UserLanguage);
            Assert.AreEqual(string.Empty, parameters.AppVersion);
            Assert.AreEqual(string.Empty, parameters.DeviceUniqueIdentifier);
            Assert.AreEqual(string.Empty, parameters.Platform);
        }

        [Test]
        public void ToJson_OutputsAllEightFields()
        {
            var parameters = new StartupHttpParams
            {
                Language = "ChineseSimplified",
                UserLanguage = "zh-CN",
                AppVersion = "1.2.3",
                DeviceUniqueIdentifier = "device-1",
                Platform = "Android",
                PackageName = "com.company.game",
                Channel = "official",
                SubChannel = "qa",
            };

            var json = parameters.ToJson();

            StringAssert.Contains("\"Language\":\"ChineseSimplified\"", json);
            StringAssert.Contains("\"UserLanguage\":\"zh-CN\"", json);
            StringAssert.Contains("\"AppVersion\":\"1.2.3\"", json);
            StringAssert.Contains("\"DeviceUniqueIdentifier\":\"device-1\"", json);
            StringAssert.Contains("\"Platform\":\"Android\"", json);
            StringAssert.Contains("\"PackageName\":\"com.company.game\"", json);
            StringAssert.Contains("\"Channel\":\"official\"", json);
            StringAssert.Contains("\"SubChannel\":\"qa\"", json);
        }

        [Test]
        public void StartupHttpParams_ImplementsInterface()
        {
            Assert.IsInstanceOf<IStartupHttpParams>(new StartupHttpParams());
        }

        [Test]
        public void StartupHttpParams_CanBeSubclassed()
        {
            var parameters = new CustomStartupHttpParams();

            var dictionary = parameters.ToDictionary();

            Assert.AreEqual("custom", dictionary["Custom"]);
        }

        [Test]
        public void CustomProvider_CanCreateCustomParams()
        {
            var provider = new CustomStartupHttpParamsProvider();
            var options = ScriptableObject.CreateInstance<StartupOptions>();

            var parameters = provider.Create(options).ToDictionary();

            Assert.AreEqual("custom", parameters["Custom"]);
        }

        [Test]
        public void Source_DoesNotReferenceChannelSdk()
        {
            var sourcePath = Path.Combine(
                Application.dataPath,
                "../Packages/com.gameframex.unity.startup/Runtime/Http/StartupHttpParams.cs");

            var source = File.ReadAllText(sourcePath);

            StringAssert.DoesNotContain("BlankGetChannel", source);
            StringAssert.DoesNotContain("GetChannelName", source);
        }

        private sealed class CustomStartupHttpParams : StartupHttpParams
        {
            public override System.Collections.Generic.Dictionary<string, object> ToDictionary()
            {
                var dictionary = base.ToDictionary();
                dictionary["Custom"] = "custom";
                return dictionary;
            }
        }

        private sealed class CustomStartupHttpParamsProvider : IStartupHttpParamsProvider
        {
            public IStartupHttpParams Create(StartupOptions options)
            {
                return new CustomStartupHttpParams();
            }
        }
    }
}
