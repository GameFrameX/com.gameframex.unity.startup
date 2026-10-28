using System;
using Cysharp.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程入口。提供静态 Run 方法，封装从启动 UI、iOS 联网授权门禁、远程信息获取到 YooAsset 补丁流、热更交接的完整 11 状态管线。
    /// </summary>
    /// <remarks>
    /// 调用契约与副作用：
    /// - <b>进程内只能成功调用一次</b>：第二次调用抛 <see cref="InvalidOperationException"/>。需要重新启动时先调用 IProcedureManager.DestroyProcedures 销毁流程状态机。
    /// - 调用即同步 <c>AssetComponent.GamePlayMode</c>（真机上 EditorSimulateMode 会归一化为 HostPlayMode/WebPlayMode），发生在任何资源加载之前。
    /// - ProcedureLauncherState 会重写 WebComponent 的 GameFrameX 租户鉴权 base header，相当于为启动后的全部 HTTP 请求做会话级鉴权。
    /// - options/uiHandler/hotfixLauncher 通过单个 <see cref="StartupContext"/> 注入 FSM BlackBoard（key 见 <see cref="BlackBoardKeys"/>）。
    /// - 返回的 UniTask 永远会 complete：业务失败走 Procedure 的 CompleteFailure，预期外异常由 StartupProcedureUtility.RunGuardedAsync 兜底路由，不会永久挂起。
    /// </remarks>
    public static class StartupRunner
    {
        /// <summary>
        /// 是否已经启动过。Run 在进程内只允许成功执行一次。
        /// </summary>
        private static bool _hasStarted;

        /// <summary>
        /// 当前启动的结果完成源。当 FSM BlackBoard 中的 StartupContext 不可用（注入缺失/损坏）时，
        /// CompleteFailure 以此兜底完成 awaiter，保证 Run 返回的 UniTask 永不挂起。
        /// </summary>
        internal static UniTaskCompletionSource<StartupResult> ActiveCompletionSource { get; set; }

        /// <summary>
        /// 会话边界对称重置（关闭 Domain Reload 的快速进 Play）：每次进入 Play 前把
        /// _hasStarted / ActiveCompletionSource 归零，等价于全新 Domain 的初始状态。
        /// </summary>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionStatics()
        {
            _hasStarted = false;
            ActiveCompletionSource = null;
        }

        /// 启动完整启动流程。
        /// </summary>
        /// <param name="options">配置资产，提供资源模式、URL 列表、热更入口、HTTP 参数等。</param>
        /// <param name="uiHandler">UI 处理实现（应用层注入）。</param>
        /// <param name="hotfixLauncher">热更启动实现（应用层注入）。</param>
        /// <param name="httpParamsProvider">HTTP 公共参数提供器。</param>
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
        /// <exception cref="InvalidOperationException">
        /// Run 已被调用过时抛出。启动流程每次进程生命周期只允许运行一次。
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
                    nameof(StartupOptions.GlobalInfoUrls));
            }

            if (_hasStarted)
            {
                throw new InvalidOperationException(
                    "StartupRunner.Run has already been called. Startup runs once per process; call IProcedureManager.DestroyProcedures before starting a new run.");
            }


            // 步骤 2: 在任何资源加载前同步资源运行模式。
            ApplyAssetPlayMode(options);

            // 步骤 3: 创建 UniTaskCompletionSource（成功/失败的 SetResult 由 Procedure 在 bootstrap-3/4 调用）
            var tcs = new UniTaskCompletionSource<StartupResult>();

            // 步骤 4: 获取框架模块
            var fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();

            // 步骤 5: Initialize 内部由 procedureManager 创建 procedure FSM
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

            // 步骤 6: 通过 IFsmManager.GetFsm<T>() 取回刚创建的 procedure FSM
            var procedureFsm = fsmManager.GetFsm<IProcedureManager>();

            // 步骤 7: 注入单个 StartupContext（VarObject 无隐式转换，显式 Acquire + 赋 Value）
            var httpParamsProviderResolved = httpParamsProvider ?? new DefaultStartupHttpParamsProvider();
            var context = new StartupContext(options, uiHandler, hotfixLauncher, tcs, httpParamsProviderResolved);
            var contextBox = ReferencePool.Acquire<VarObject>();
            contextBox.Value = context;
            procedureFsm.SetData(BlackBoardKeys.StartupContext, contextBox);

            // 步骤 8: 标记已启动并启动第一个 Procedure 状态
            _hasStarted = true;
            ActiveCompletionSource = tcs;
            procedureManager.StartProcedure<ProcedureLauncherState>();

            // 步骤 9: 返回 tcs.Task（fire-and-forget 启动后立即返回；成功/失败由 Procedure 调用 CompleteSuccess/CompleteFailure 完成）
            return tcs.Task;
        }

        private static void ApplyAssetPlayMode(StartupOptions options)
        {
            GameEntry.GetComponent<AssetComponent>().GamePlayMode = NormalizeAssetPlayMode(options.GamePlayMode);
        }

        private static EPlayMode NormalizeAssetPlayMode(EPlayMode playMode)
        {
#if !UNITY_EDITOR
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                playMode = EPlayMode.HostPlayMode;
            }
#if UNITY_WEBGL
            playMode = EPlayMode.WebPlayMode;
#endif
#endif
            return playMode;
        }
    }
}
