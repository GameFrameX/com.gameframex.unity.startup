using System;

using Cysharp.Threading.Tasks;

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程入口。提供静态 Run 方法，封装完整的启动流程。
    /// </summary>
    /// <remarks>
    /// **本 bootstrap-1 阶段为入口骨架实现**：
    /// - 同步配置校验（AC-2）
    /// - 通过 FSM BlackBoard 注入 options/uiHandler/hotfixLauncher（AC-14）
    /// - 启动 ProcedureLauncherState（AC-1，stub 阶段不前进）
    /// - 返回 UniTask&lt;StartupResult&gt;（成功/失败的 SetResult 由后续 bootstrap 阶段在 Procedure 内部调用）
    /// </remarks>
    public static class StartupRunner
    {
        /// <summary>
        /// 启动完整启动流程。
        /// </summary>
        /// <param name="options">配置资产，提供 URL 列表、热更入口、HTTP 参数等。</param>
        /// <param name="uiHandler">UI 处理实现（应用层注入）。</param>
        /// <param name="hotfixLauncher">热更启动实现（应用层注入）。</param>
        /// <returns>
        /// UniTask&lt;StartupResult&gt; — 永远会 complete（成功/失败都返回，不抛异常）。
        /// 调用方可 await 此 UniTask；同时流程结束时会通过 GameApp.Event.Fire 触发 StartupCompleted/FailedEventArgs。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// options/uiHandler/hotfixLauncher 任一为 null 时同步抛出。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// options.GlobalInfoUrls 为 null 或空数组时同步抛出（消息含 "GlobalInfoUrls"）。
        /// 此异常在 await 之前抛出，不进入 async 状态机。
        /// </exception>
        public static UniTask<StartupResult> Run(
            StartupOptions options,
            IStartupUIHandler uiHandler,
            IHotfixLauncher hotfixLauncher,
            IStartupHttpParamsProvider httpParamsProvider = null)
        {
            // 步骤 1: 同步配置校验（不进入 async 状态机）
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (uiHandler == null)
            {
                throw new ArgumentNullException(nameof(uiHandler));
            }

            if (hotfixLauncher == null)
            {
                throw new ArgumentNullException(nameof(hotfixLauncher));
            }

            if (options.GlobalInfoUrls == null || options.GlobalInfoUrls.Length == 0)
            {
                throw new ArgumentException(
                    "StartupOptions.GlobalInfoUrls must contain at least one URL.",
                    "GlobalInfoUrls");
            }

            // 步骤 2: 创建 UniTaskCompletionSource（成功/失败的 SetResult 由 Procedure 在 bootstrap-3/4 调用）
            var tcs = new UniTaskCompletionSource<StartupResult>();

            // 步骤 3: 获取框架模块
            var fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();

            // 步骤 4: Initialize 内部由 procedureManager 创建 procedure FSM
            procedureManager.Initialize(fsmManager, new ProcedureBase[]
            {
                new ProcedureLauncherState(),
                new ProcedureGetGlobalInfoState(),
                new ProcedureGetAppVersionInfoState(),
                new ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState(),
                new ProcedurePatchInit(),
                new ProcedureUpdateStaticVersion(),
                new ProcedureUpdateManifest(),
                new ProcedureCreateDownloader(),
                new ProcedureDownloadWebFiles(),
                new ProcedurePatchDone(),
                new ProcedureGameLauncherState(),
            });

            // 步骤 5: 通过 IFsmManager.GetFsm<T>() 取回刚创建的 procedure FSM
            var procedureFsm = fsmManager.GetFsm<IProcedureManager>();

            // 步骤 6: 注入 3 个 BlackBoard key（VarObject 无隐式转换，显式 Acquire + 赋 Value）
            var optionsBox = ReferencePool.Acquire<VarObject>();
            optionsBox.Value = options;
            procedureFsm.SetData(BlackBoardKeys.StartupOptions, optionsBox);

            var uiHandlerBox = ReferencePool.Acquire<VarObject>();
            uiHandlerBox.Value = uiHandler;
            procedureFsm.SetData(BlackBoardKeys.StartupUIHandler, uiHandlerBox);

            var hotfixLauncherBox = ReferencePool.Acquire<VarObject>();
            hotfixLauncherBox.Value = hotfixLauncher;
            procedureFsm.SetData(BlackBoardKeys.StartupHotfixLauncher, hotfixLauncherBox);

            var completionSourceBox = ReferencePool.Acquire<VarObject>();
            completionSourceBox.Value = tcs;
            procedureFsm.SetData(BlackBoardKeys.StartupCompletionSource, completionSourceBox);

            var httpParamsProviderBox = ReferencePool.Acquire<VarObject>();
            httpParamsProviderBox.Value = httpParamsProvider ?? new DefaultStartupHttpParamsProvider();
            procedureFsm.SetData(BlackBoardKeys.StartupHttpParamsProvider, httpParamsProviderBox);

            // 步骤 7: 启动第一个 Procedure 状态
            procedureManager.StartProcedure<ProcedureLauncherState>();

            // 步骤 8: 返回 tcs.Task（fire-and-forget 启动后立即返回；tcs 由 Procedure 在 bootstrap-3/4 完成）
            return tcs.Task;
        }
    }
}
