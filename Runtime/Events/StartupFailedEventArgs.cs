using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

using UnityEngine.Scripting;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程失败事件。任一 Procedure 因网络或业务失败导致流程停止时触发。
    /// </summary>
    [Preserve]
    public sealed class StartupFailedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 事件 ID（typeof(X).FullName 模式，与 GameFrameX 全家桶统一）。
        /// </summary>
        public static readonly string EventId = typeof(StartupFailedEventArgs).FullName;

        /// <inheritdoc />
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 失败发生所在的 Procedure 状态名。
        /// </summary>
        public string FailedProcedureName { get; private set; }

        /// <summary>
        /// 最后一个尝试失败的 URL（非网络失败时为空字符串）。
        /// </summary>
        public string FailedUrl { get; private set; }

        /// <summary>
        /// 错误描述。
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 从引用池获取实例并填充字段。
        /// </summary>
        /// <param name="procedureName">失败 Procedure 状态名。</param>
        /// <param name="url">最后尝试失败的 URL（非网络失败时传空字符串）。</param>
        /// <param name="errorMessage">错误描述。</param>
        public static StartupFailedEventArgs Create(string procedureName, string url, string errorMessage)
        {
            var args = ReferencePool.Acquire<StartupFailedEventArgs>();
            args.FailedProcedureName = procedureName ?? string.Empty;
            args.FailedUrl = url ?? string.Empty;
            args.ErrorMessage = errorMessage ?? string.Empty;
            return args;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            FailedProcedureName = string.Empty;
            FailedUrl = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}
