using System.Collections.Generic;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动HTTP参数接口。
    /// </summary>
    /// <remarks>
    /// Interface for startup HTTP parameters.
    /// </remarks>
    public interface IStartupHttpParams
    {
        /// <summary>
        /// 将参数序列化为JSON字符串。
        /// </summary>
        /// <remarks>
        /// Serialize parameters to JSON string.
        /// </remarks>
        /// <returns>JSON字符串 / JSON string</returns>
        string ToJson();

        /// <summary>
        /// 将参数转换为字典。
        /// </summary>
        /// <remarks>
        /// Converts parameters to dictionary.
        /// </remarks>
        /// <returns>参数字典 / Parameters dictionary</returns>
        Dictionary<string, object> ToDictionary();
    }
}