using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程配置资产。所有项目可变项（资源模式、URL 主备列表、热更入口、HTTP 公共参数、UI 资源路径）通过此 SO 注入。
    /// </summary>
    [CreateAssetMenu(menuName = "GameFrameX/Startup Options", fileName = "StartupOptions")]
    public sealed class StartupOptions : ScriptableObject
    {
        [Header("Admin")] [Tooltip("GameFrameX 管理后台的租户ID")]
        public string GameFrameXApiKey = "";

        [Tooltip("GameFrameX 管理后台的应用ID")] public string GameFrameXAppId = "";
        [Tooltip("GameFrameX 管理后台的应用密钥")] public string GameFrameXAppSecret = "";

        [Header("Asset")] [Tooltip("资源运行模式。启动流程会在加载资源前同步到 Asset 组件。")]
        public EPlayMode GamePlayMode = EPlayMode.EditorSimulateMode;

        [Header("Network")] [Tooltip("全局信息接口 URL 主备列表，按顺序尝试。空数组视为配置错误。")]
        public string[] GlobalInfoUrls = new string[0];


        [Tooltip("每个 URL 内部的总尝试次数上限（含初次）。")] public int MaxAttemptsPerUrl = 3;

        [Tooltip("重试之间的延迟毫秒数。")] public int RetryDelayMs = 3000;

        [Tooltip("是否跳过远程启动请求（全局信息、App 版本、资源包版本）。WebGL 单机等无后端场景启用，直接进入本地资源初始化。")]
        public bool SkipRemoteStartupRequests = false;

        [Header("Hotfix")] [Tooltip("Hotfix 程序集名，传给 IHotfixLauncher 使用。")]
        public string HotfixAssemblyName = "Unity.Hotfix";

        [Tooltip("Hotfix 入口类型全名。")] public string HotfixEntryTypeName = "Hotfix.HotfixLauncher";

        [Tooltip("Hotfix 入口方法名。")] public string HotfixEntryMethodName = "Main";

        [Header("HTTP Base Params")] [Tooltip("HTTP 公共参数中的应用包名（如 com.company.game）。运行时覆盖默认值。")]
        public string PackageName = string.Empty;

        [Tooltip("HTTP 公共参数中的渠道标识。运行时覆盖默认值。")] public string Channel = string.Empty;

        [Tooltip("HTTP 公共参数中的子渠道标识。运行时覆盖默认值。")]
        public string SubChannel = string.Empty;

        [Header("UI")]
        [Tooltip("启动 UI 的资源路径，传给 IStartupUIHandler。")]
        public string LauncherUIResName = "UI/UILauncher";
    }
}