namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// FSM BlackBoard key 常量。StartupRunner 通过这 3 个 key 把 options/uiHandler/hotfixLauncher 注入 procedure FSM，
    /// 所有 Procedure 状态通过 IFsm&lt;IProcedureManager&gt;.GetData&lt;VarObject&gt;(key) 读取。
    /// </summary>
    public static class BlackBoardKeys
    {
        /// <summary>
        /// StartupOptions 实例的 BlackBoard key。
        /// </summary>
        public const string StartupOptions = "__startup_options__";

        /// <summary>
        /// IStartupUIHandler 实例的 BlackBoard key。
        /// </summary>
        public const string StartupUIHandler = "__startup_ui_handler__";

        /// <summary>
        /// IHotfixLauncher 实例的 BlackBoard key。
        /// </summary>
        public const string StartupHotfixLauncher = "__startup_hotfix_launcher__";

        /// <summary>
        /// UniTaskCompletionSource&lt;StartupResult&gt; 实例的 BlackBoard key。
        /// </summary>
        public const string StartupCompletionSource = "__startup_completion_source__";

        /// <summary>
        /// IStartupHttpParamsProvider 实例的 BlackBoard key。
        /// </summary>
        public const string StartupHttpParamsProvider = "__startup_http_params_provider__";
    }
}
