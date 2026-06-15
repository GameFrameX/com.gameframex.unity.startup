using System;
using System.Collections.Generic;

using GameFrameX.Startup.Runtime;

using UnityEngine;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// HTTP base parameters used by startup requests.
    /// </summary>
    [Serializable]
    public class StartupHttpParams : IStartupHttpParams
    {
        public string Language = string.Empty;

        public string UserLanguage = string.Empty;

        public string AppVersion = string.Empty;

        public string DeviceUniqueIdentifier = string.Empty;

        public string Platform = string.Empty;

        public string PackageName = string.Empty;

        public string Channel = string.Empty;

        public string SubChannel = string.Empty;

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

            return JsonUtility.ToJson(this);
        }

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
