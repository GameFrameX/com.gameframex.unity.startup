namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 热更加载结果。IHotfixLauncher.StartAsync 的返回类型。
    /// </summary>
    public sealed class HotfixLaunchResult
    {
        /// <summary>
        /// 热更加载是否成功。
        /// </summary>
        public bool Success;

        /// <summary>
        /// 失败时填，错误描述；成功时为空字符串。
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// 构造成功结果。
        /// </summary>
        public static HotfixLaunchResult Succeed()
        {
            return new HotfixLaunchResult
            {
                Success = true,
                ErrorMessage = string.Empty,
            };
        }

        /// <summary>
        /// 构造失败结果。
        /// </summary>
        /// <param name="errorMessage">错误描述。</param>
        public static HotfixLaunchResult Fail(string errorMessage)
        {
            return new HotfixLaunchResult
            {
                Success = false,
                ErrorMessage = errorMessage ?? string.Empty,
            };
        }
    }
}
