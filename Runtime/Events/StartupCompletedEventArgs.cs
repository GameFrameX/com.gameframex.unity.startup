using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

using UnityEngine.Scripting;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程成功完成事件。ProcedureGameLauncherState 完成热更加载 + UI Dispose 后触发。
    /// </summary>
    [Preserve]
    public sealed class StartupCompletedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 事件 ID（typeof(X).FullName 模式，与 GameFrameX 全家桶统一）。
        /// </summary>
        public static readonly string EventId = typeof(StartupCompletedEventArgs).FullName;

        /// <inheritdoc />
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 从引用池获取实例。无额外字段需要填充。
        /// </summary>
        public static StartupCompletedEventArgs Create()
        {
            return ReferencePool.Acquire<StartupCompletedEventArgs>();
        }

        /// <inheritdoc />
        public override void Clear()
        {
            // 无字段，空实现
        }
    }
}
