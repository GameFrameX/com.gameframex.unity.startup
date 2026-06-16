namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 默认的启动HTTP参数提供者。
    /// </summary>
    /// <remarks>
    /// Default provider for startup HTTP parameters.
    /// </remarks>
    public class DefaultStartupHttpParamsProvider : IStartupHttpParamsProvider
    {
        /// <summary>
        /// 根据启动选项创建HTTP参数。
        /// </summary>
        /// <remarks>
        /// Creates HTTP parameters from startup options.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>HTTP参数实例 / HTTP parameters instance</returns>
        public virtual IStartupHttpParams Create(StartupOptions options)
        {
            return StartupHttpParams.FromOptions(options);
        }
    }
}
