namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程结果。UniTask&lt;StartupResult&gt; 的返回类型，也用于事件参数构造。
    /// </summary>
    public sealed class StartupResult
    {
        /// <summary>
        /// 流程是否成功。
        /// </summary>
        public bool Success;

        /// <summary>
        /// 失败时填，失败发生所在的 Procedure 状态名（成功时为空字符串）。
        /// </summary>
        public string FailedProcedureName;

        /// <summary>
        /// 失败时填，最后一个尝试失败的 URL（成功或非网络失败时为空字符串）。
        /// </summary>
        public string FailedUrl;

        /// <summary>
        /// 失败时填，错误描述。
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// 构造成功结果。
        /// </summary>
        public static StartupResult Succeed()
        {
            return new StartupResult
            {
                Success = true,
                FailedProcedureName = string.Empty,
                FailedUrl = string.Empty,
                ErrorMessage = string.Empty,
            };
        }

        /// <summary>
        /// 构造失败结果。
        /// </summary>
        /// <param name="procedureName">失败 Procedure 状态名。</param>
        /// <param name="url">最后尝试失败的 URL（非网络失败时传空字符串）。</param>
        /// <param name="errorMessage">错误描述。</param>
        public static StartupResult Fail(string procedureName, string url, string errorMessage)
        {
            return new StartupResult
            {
                Success = false,
                FailedProcedureName = procedureName ?? string.Empty,
                FailedUrl = url ?? string.Empty,
                ErrorMessage = errorMessage ?? string.Empty,
            };
        }
    }
}
