using System;

using Cysharp.Threading.Tasks;

using GameFrameX.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程上下文。聚合 StartupRunner 注入的全部运行时依赖，通过单个 BlackBoard key 传入 procedure FSM，
    /// 取代此前分散的多个字符串 key 服务定位。
    /// </summary>
    /// <remarks>
    /// Startup context. Aggregates all runtime dependencies injected by StartupRunner and is passed into the
    /// procedure FSM through a single blackboard key, replacing the previous scattered string-keyed service lookup.
    /// </remarks>
    public sealed class StartupContext
    {
        /// <summary>
        /// 构造启动上下文。所有依赖必须非空（由 StartupRunner.Run 的参数校验保证）。
        /// </summary>
        /// <param name="options">启动配置资产。</param>
        /// <param name="uiHandler">UI 处理实现。</param>
        /// <param name="hotfixLauncher">热更启动实现。</param>
        /// <param name="completionSource">启动结果完成源，由 Procedure 在成功/失败时 complete。</param>
        /// <param name="httpParamsProvider">HTTP 公共参数提供器。</param>
        public StartupContext(
            StartupOptions options,
            IStartupUIHandler uiHandler,
            IHotfixLauncher hotfixLauncher,
            UniTaskCompletionSource<StartupResult> completionSource,
            IStartupHttpParamsProvider httpParamsProvider)
        {
            GameFrameworkGuard.NotNull(options, nameof(options));
            GameFrameworkGuard.NotNull(uiHandler, nameof(uiHandler));
            GameFrameworkGuard.NotNull(hotfixLauncher, nameof(hotfixLauncher));
            GameFrameworkGuard.NotNull(completionSource, nameof(completionSource));
            GameFrameworkGuard.NotNull(httpParamsProvider, nameof(httpParamsProvider));

            Options = options;
            UIHandler = uiHandler;
            HotfixLauncher = hotfixLauncher;
            CompletionSource = completionSource;
            HttpParamsProvider = httpParamsProvider;
        }

        /// <summary>
        /// 启动配置资产。
        /// </summary>
        public StartupOptions Options { get; }

        /// <summary>
        /// UI 处理实现。
        /// </summary>
        public IStartupUIHandler UIHandler { get; }

        /// <summary>
        /// 热更启动实现。
        /// </summary>
        public IHotfixLauncher HotfixLauncher { get; }

        /// <summary>
        /// 启动结果完成源。
        /// </summary>
        public UniTaskCompletionSource<StartupResult> CompletionSource { get; }

        /// <summary>
        /// HTTP 公共参数提供器。
        /// </summary>
        public IStartupHttpParamsProvider HttpParamsProvider { get; }
    }
}
