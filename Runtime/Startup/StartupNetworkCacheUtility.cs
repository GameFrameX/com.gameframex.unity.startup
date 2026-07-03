using System;

using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动网络缓存工具类。
    /// </summary>
    /// <remarks>
    /// Startup network cache utility class. Provides methods to save and retrieve cached network response data
    /// during the startup procedure, reducing network requests on subsequent launches.
    /// </remarks>
    internal static class StartupNetworkCacheUtility
    {
        private const string CachePrefix = "GameFrameX.Startup.NetworkCache";
        private const string GlobalInfoKey = CachePrefix + ".GlobalInfo";
        private const string AppVersionKey = CachePrefix + ".AppVersion";
        private const string AssetPackageVersionKey = CachePrefix + ".AssetPackageVersion";

        /// <summary>
        /// 保存全局信息到缓存。
        /// </summary>
        /// <remarks>
        /// Saves the global info response to the local cache.
        /// </remarks>
        /// <param name="responseJson">全局信息响应JSON字符串 / Global info response JSON string</param>
        public static void SaveGlobalInfo(string responseJson)
        {
            SaveString(GlobalInfoKey, responseJson);
        }

        /// <summary>
        /// 尝试应用缓存的全局信息。
        /// </summary>
        /// <remarks>
        /// Attempts to apply the cached global info. Returns true if cache exists and is successfully applied.
        /// </remarks>
        /// <returns>如果缓存存在且成功应用则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if cache exists and is successfully applied; otherwise <c>false</c></returns>
        public static bool TryApplyCachedGlobalInfo()
        {
            if (!TryGetString(GlobalInfoKey, out var responseJson))
            {
                return false;
            }

            try
            {
                var responseGlobalInfo = responseJson.ToHttpJsonResultData<ResponseGlobalInfo>();
                if (!responseGlobalInfo.IsSuccess)
                {
                    return false;
                }

                ApplyGlobalInfo(responseJson, responseGlobalInfo.Data);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }

        /// <summary>
        /// 应用全局信息到游戏配置。
        /// </summary>
        /// <remarks>
        /// Applies the global info to the game configuration, including version check URLs and content data.
        /// </remarks>
        /// <param name="responseJson">全局信息响应JSON字符串 / Global info response JSON string</param>
        /// <param name="data">全局信息数据对象 / Global info data object</param>
        public static void ApplyGlobalInfo(string responseJson, ResponseGlobalInfo data)
        {
            var globalConfig = GameApp.GlobalConfig;
            globalConfig.SetOriginalData(responseJson);
            globalConfig.CheckAppVersionUrl = data.CheckAppVersionUrl;
            globalConfig.CheckResourceVersionUrl = data.CheckResourceVersionUrl;
            globalConfig.Content = data.Content;
            globalConfig.SetGlobalConfig(data);
        }

        /// <summary>
        /// 保存应用版本信息到缓存。
        /// </summary>
        /// <remarks>
        /// Saves the application version info to the local cache.
        /// </remarks>
        /// <param name="dataJson">应用版本信息JSON字符串 / Application version info JSON string</param>
        public static void SaveAppVersionInfo(string dataJson)
        {
            SaveString(AppVersionKey, dataJson);
        }

        /// <summary>
        /// 尝试从缓存获取应用版本信息。
        /// </summary>
        /// <remarks>
        /// Attempts to retrieve the cached application version info.
        /// </remarks>
        /// <param name="gameAppVersion">应用版本信息 / Application version info</param>
        /// <returns>如果缓存存在且成功解析则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if cache exists and is successfully parsed; otherwise <c>false</c></returns>
        public static bool TryGetCachedAppVersionInfo(out ResponseGameAppVersion gameAppVersion)
        {
            gameAppVersion = null;
            if (!TryGetString(AppVersionKey, out var dataJson))
            {
                return false;
            }

            try
            {
                gameAppVersion = Utility.Json.ToObject<ResponseGameAppVersion>(dataJson);
                return gameAppVersion != null;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }

        /// <summary>
        /// 保存资源包版本信息到缓存。
        /// </summary>
        /// <remarks>
        /// Saves the asset package version info to the local cache.
        /// </remarks>
        /// <param name="dataJson">资源包版本信息JSON字符串 / Asset package version info JSON string</param>
        public static void SaveAssetPackageVersionInfo(string dataJson)
        {
            SaveString(AssetPackageVersionKey, dataJson);
        }

        /// <summary>
        /// 尝试应用缓存的资源包版本信息。
        /// </summary>
        /// <remarks>
        /// Attempts to apply the cached asset package version info to the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者，用于存储资源包信息 / Procedure owner, used to store asset package info</param>
        /// <returns>如果缓存存在且成功应用则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if cache exists and is successfully applied; otherwise <c>false</c></returns>
        public static bool TryApplyCachedAssetPackageVersionInfo(IFsm<IProcedureManager> procedureOwner)
        {
            if (!TryGetString(AssetPackageVersionKey, out var dataJson))
            {
                return false;
            }

            try
            {
                var packageVersion = Utility.Json.ToObject<ResponseGameAssetPackageVersion>(dataJson);
                if (packageVersion == null)
                {
                    return false;
                }

                ApplyAssetPackageVersionInfo(procedureOwner, packageVersion);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }

        /// <summary>
        /// 应用资源包版本信息到流程。
        /// </summary>
        /// <remarks>
        /// Applies the asset package version info to the procedure owner by using the server provided package path,
        /// or constructing one from root path, package name, platform, app version, channel, asset package name, and version.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者，用于存储资源包信息 / Procedure owner, used to store asset package info</param>
        /// <param name="packageVersion">资源包版本信息 / Asset package version info</param>
        public static void ApplyAssetPackageVersionInfo(IFsm<IProcedureManager> procedureOwner, ResponseGameAssetPackageVersion packageVersion)
        {
            var packageUrl = GetAssetPackageUrl(packageVersion);

            var urlValue = ReferencePool.Acquire<VarString>();
            urlValue.SetValue(packageUrl);
            procedureOwner.SetData(AssetComponent.BuildInPackageName, urlValue);

            var versionValue = ReferencePool.Acquire<VarString>();
            versionValue.SetValue(packageVersion.Version);
            procedureOwner.SetData(AssetComponent.BuildInPackageName + "Version", versionValue);
        }

        private static string GetAssetPackageUrl(ResponseGameAssetPackageVersion packageVersion)
        {
            if (!string.IsNullOrWhiteSpace(packageVersion.AssetPackagePath))
            {
                return packageVersion.AssetPackagePath;
            }

            return EnsureTrailingSlash(PathHelper.Combine(
                packageVersion.RootPath,
                packageVersion.PackageName,
                packageVersion.Platform,
                packageVersion.AppVersion,
                packageVersion.Channel,
                packageVersion.AssetPackageName,
                packageVersion.Version));
        }

        private static string EnsureTrailingSlash(string path)
        {
            if (path.EndsWithFast("/") || path.EndsWithFast("\\"))
            {
                return path;
            }

            return path + "/";
        }

        private static bool TryGetString(string key, out string value)
        {
            value = string.Empty;
#if ENABLE_GAME_FRAME_X_SETTING
            var setting = GameApp.Setting;
            if (setting == null || !setting.HasSetting(key))
            {
                return false;
            }

            value = setting.GetString(key, string.Empty);
            return !string.IsNullOrEmpty(value);
#else
            Log.Error("Startup network cache requires com.gameframex.unity.setting.");
            return false;
#endif
        }

        private static void SaveString(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

#if ENABLE_GAME_FRAME_X_SETTING
            var setting = GameApp.Setting;
            if (setting == null)
            {
                return;
            }

            setting.SetString(key, value);
            setting.Save();
#else
            Log.Error("Startup network cache requires com.gameframex.unity.setting.");
#endif
        }
    }
}
