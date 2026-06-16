namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动HTTP参数提供者接口。
    /// </summary>
    /// <remarks>
    /// Interface for providing startup HTTP parameters.
    /// </remarks>
    public interface IStartupHttpParamsProvider
    {
        /// <summary>
        /// 根据启动选项创建HTTP参数。
        /// </summary>
        /// <remarks>
        /// Creates HTTP parameters from startup options.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>HTTP参数实例 / HTTP parameters instance</returns>
        IStartupHttpParams Create(StartupOptions options);
    }
}
