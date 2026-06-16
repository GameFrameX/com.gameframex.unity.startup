using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using UnityEngine;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// HTTP base parameters used by startup requests.
    /// </summary>
    /// <summary>
    /// 启动HTTP参数，用于启动请求的基础参数。
    /// </summary>
    /// <remarks>
    /// HTTP base parameters used by startup requests.
    /// </remarks>
    [Serializable]
    public class StartupHttpParams : IStartupHttpParams
    {
        /// <summary>
        /// 语言代码。
        /// </summary>
        /// <remarks>
        /// Language code.
        /// </remarks>
        public string Language = string.Empty;

        /// <summary>
        /// 用户语言。
        /// </summary>
        /// <remarks>
        /// User language.
        /// </remarks>
        public string UserLanguage = string.Empty;

        /// <summary>
        /// 应用程序版本。
        /// </summary>
        /// <remarks>
        /// Application version.
        /// </remarks>
        public string AppVersion = string.Empty;

        /// <summary>
        /// 设备唯一标识符。
        /// </summary>
        /// <remarks>
        /// Device unique identifier.
        /// </remarks>
        public string DeviceUniqueIdentifier = string.Empty;

        /// <summary>
        /// 平台标识。
        /// </summary>
        /// <remarks>
        /// Platform identifier.
        /// </remarks>
        public string Platform = string.Empty;

        /// <summary>
        /// 包名。
        /// </summary>
        /// <remarks>
        /// Package name.
        /// </remarks>
        public string PackageName = string.Empty;

        /// <summary>
        /// 渠道标识。
        /// </summary>
        /// <remarks>
        /// Channel identifier.
        /// </remarks>
        public string Channel = string.Empty;

        /// <summary>
        /// 子渠道标识。
        /// </summary>
        /// <remarks>
        /// Sub-channel identifier.
        /// </remarks>
        public string SubChannel = string.Empty;

        /// <summary>
        /// 从启动选项创建HTTP参数实例。
        /// </summary>
        /// <remarks>
        /// Creates HTTP parameters instance from startup options.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>HTTP参数实例 / HTTP parameters instance</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 null 时抛出 / Thrown when <paramref name="options"/> is null</exception>
        public static StartupHttpParams FromOptions(StartupOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new StartupHttpParams
            {
                PackageName = options.PackageName ?? string.Empty,
                Channel = options.Channel ?? string.Empty,
                SubChannel = options.SubChannel ?? string.Empty,
            };
        }

        /// <summary>
        /// 将参数序列化为JSON字符串。
        /// </summary>
        /// <remarks>
        /// Serializes parameters to JSON string.
        /// </remarks>
        /// <returns>JSON字符串 / JSON string</returns>
        public virtual string ToJson()
        {
            Language = Language ?? string.Empty;
            UserLanguage = UserLanguage ?? string.Empty;
            AppVersion = AppVersion ?? string.Empty;
            DeviceUniqueIdentifier = DeviceUniqueIdentifier ?? string.Empty;
            Platform = Platform ?? string.Empty;
            PackageName = PackageName ?? string.Empty;
            Channel = Channel ?? string.Empty;
            SubChannel = SubChannel ?? string.Empty;

            return Utility.Json.ToJson(this);
        }

        /// <summary>
        /// 将参数转换为字典。
        /// </summary>
        /// <remarks>
        /// Converts parameters to dictionary.
        /// </remarks>
        /// <returns>参数字典 / Parameters dictionary</returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["Language"] = Language ?? string.Empty,
                ["UserLanguage"] = UserLanguage ?? string.Empty,
                ["AppVersion"] = AppVersion ?? string.Empty,
                ["DeviceUniqueIdentifier"] = DeviceUniqueIdentifier ?? string.Empty,
                ["Platform"] = Platform ?? string.Empty,
                ["PackageName"] = PackageName ?? string.Empty,
                ["Channel"] = Channel ?? string.Empty,
                ["SubChannel"] = SubChannel ?? string.Empty,
            };
        }
    }
}