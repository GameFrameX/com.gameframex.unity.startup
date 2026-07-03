using System;
using System.Reflection;

using GameFrameX.Startup.Runtime;

using NUnit.Framework;

namespace GameFrameX.Startup.Runtime.Tests
{
    [TestFixture]
    internal sealed class StartupNetworkCacheUtilityTests
    {
        [Test]
        public void GetAssetPackageUrl_UsesServerProvidedPathFirst()
        {
            var response = CreatePackageVersion();
            Set(response, "AssetPackagePath", "https://cdn.example.com/full/path");
            Set(response, "RootPath", "https://wrong.example.com");
            Set(response, "PackageName", "wrong.package");
            Set(response, "Platform", "WrongPlatform");
            Set(response, "AppVersion", "0.0.0");
            Set(response, "Channel", "wrong");
            Set(response, "AssetPackageName", "WrongPackage");
            Set(response, "Version", "0");

            Assert.AreEqual("https://cdn.example.com/full/path", InvokeGetAssetPackageUrl(response));
        }

        [Test]
        public void GetAssetPackageUrl_FallsBackToLegacyFields()
        {
            var response = CreatePackageVersion();
            Set(response, "RootPath", "https://cdn.example.com");
            Set(response, "PackageName", "com.company.game");
            Set(response, "Platform", "Android");
            Set(response, "AppVersion", "1.0.0");
            Set(response, "Channel", "official");
            Set(response, "AssetPackageName", "DefaultPackage");
            Set(response, "Version", "100");

            Assert.AreEqual(
                "https://cdn.example.com/com.company.game/Android/1.0.0/official/DefaultPackage/100/",
                InvokeGetAssetPackageUrl(response));
        }

        private static object CreatePackageVersion()
        {
            return Activator.CreateInstance(GetPackageVersionType());
        }

        private static Type GetPackageVersionType()
        {
            return GetMethod().GetParameters()[0].ParameterType;
        }

        private static string InvokeGetAssetPackageUrl(object response)
        {
            return (string)GetMethod().Invoke(null, new[] { response });
        }

        private static MethodInfo GetMethod()
        {
            var type = typeof(StartupOptions).Assembly.GetType("GameFrameX.Startup.Runtime.StartupNetworkCacheUtility");
            return type.GetMethod("GetAssetPackageUrl", BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static void Set(object instance, string propertyName, string value)
        {
            instance.GetType().GetProperty(propertyName).SetValue(instance, value);
        }
    }
}
