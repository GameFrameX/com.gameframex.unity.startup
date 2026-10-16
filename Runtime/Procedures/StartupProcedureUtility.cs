using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Localization.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程工具类，提供启动相关的数据读取、结果通知和进度更新等辅助方法。
    /// </summary>
    /// <remarks>
    /// Startup procedure utility class, provides helper methods for reading startup data, notifying results and updating download progress.
    /// </remarks>
    internal static class StartupProcedureUtility
    {
        public const string GameFrameXApiKeyHeader = "GameFrameX-Tenant-Id";
        public const string GameFrameXAppIdHeader = "GameFrameX-App-Id";
        public const string GameFrameXAppSecretHeader = "GameFrameX-App-Secret";
        public const string GameFrameXTenantSecretHeader = "GameFrameX-Tenant-Secret";

        /// <summary>
        /// 从流程所有者中获取启动上下文。
        /// </summary>
        /// <remarks>
        /// Gets the startup context from the procedure owner. Throws a descriptive exception when the context has
        /// not been injected, i.e. the procedure FSM was not started through StartupRunner.Run.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>启动上下文实例 / Startup context instance</returns>
        public static StartupContext GetContext(IFsm<IProcedureManager> procedureOwner)
        {
            var contextBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupContext);
            if (contextBox != null && contextBox.Value is StartupContext context)
            {
                return context;
            }

            throw new GameFrameworkException("Startup context is missing from the procedure FSM blackboard (key: " + BlackBoardKeys.StartupContext + "). Start the flow via StartupRunner.Run so the context gets injected.");
        }

        /// <summary>
        /// 从流程所有者中获取启动选项。
        /// </summary>
        /// <remarks>
        /// Gets the startup options from the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>启动选项实例 / Startup options instance</returns>
        public static StartupOptions GetOptions(IFsm<IProcedureManager> procedureOwner)
        {
            return GetContext(procedureOwner).Options;
        }

        /// <summary>
        /// 从流程所有者中获取启动界面处理器。
        /// </summary>
        /// <remarks>
        /// Gets the startup UI handler from the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>启动界面处理器实例 / Startup UI handler instance</returns>
        public static IStartupUIHandler GetUIHandler(IFsm<IProcedureManager> procedureOwner)
        {
            return GetContext(procedureOwner).UIHandler;
        }

        /// <summary>
        /// 从流程所有者中获取异步结果完成源。
        /// </summary>
        /// <remarks>
        /// Gets the async result completion source from the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>异步结果完成源 / Async result completion source</returns>
        public static UniTaskCompletionSource<StartupResult> GetCompletionSource(IFsm<IProcedureManager> procedureOwner)
        {
            return GetContext(procedureOwner).CompletionSource;
        }

        /// <summary>
        /// 从流程所有者中获取 HTTP 参数提供器。
        /// </summary>
        /// <remarks>
        /// Gets the HTTP params provider from the procedure owner. Run 时已保证非空。
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>HTTP 参数提供器实例 / HTTP params provider instance</returns>
        public static IStartupHttpParamsProvider GetHttpParamsProvider(IFsm<IProcedureManager> procedureOwner)
        {
            return GetContext(procedureOwner).HttpParamsProvider;
        }

        /// <summary>
        /// 在异常兜底内执行流程异步主体。捕获任何预期外异常并路由为启动失败，
        /// 保证 Run 的 awaiter 永远收到结果而不是永久挂起。
        /// </summary>
        /// <remarks>
        /// Runs the procedure body with an exception guard. Any unexpected exception is routed to
        /// CompleteFailure so the awaiter of StartupRunner.Run always receives a result instead of hanging.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <param name="procedureName">流程名，用于失败诊断 / Procedure name for failure diagnostics</param>
        /// <param name="body">流程异步主体 / Async procedure body</param>
        /// <returns>兜底执行协程 / Guarded execution coroutine</returns>
        public static async UniTask RunGuardedAsync(IFsm<IProcedureManager> procedureOwner, string procedureName, Func<UniTask> body)
        {
            try
            {
                await body();
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                CompleteFailure(procedureOwner, procedureName, string.Empty, exception.Message);
            }
        }

        /// <summary>
        /// 使用默认 HTTP 参数提供者创建 HTTP 请求参数字典。
        /// </summary>
        /// <remarks>
        /// Creates HTTP request parameters dictionary using the default provider.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>HTTP 请求参数字典 / HTTP request parameters dictionary</returns>
        public static Dictionary<string, object> CreateHttpParams(StartupOptions options)
        {
            return CreateHttpParams(options, new DefaultStartupHttpParamsProvider());
        }

        /// <summary>
        /// 使用指定的 HTTP 参数提供者创建 HTTP 请求参数字典。
        /// </summary>
        /// <remarks>
        /// Creates HTTP request parameters dictionary using the specified provider.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <param name="provider">HTTP 参数提供者 / HTTP params provider</param>
        /// <returns>HTTP 请求参数字典 / HTTP request parameters dictionary</returns>
        public static Dictionary<string, object> CreateHttpParams(StartupOptions options, IStartupHttpParamsProvider provider)
        {
            var parameters = provider.Create(options);
            ApplyRuntimeDefaults(parameters);
            return parameters.ToDictionary();
        }

        public static Dictionary<string, string> CreateGameFrameXHeaders(StartupOptions options)
        {
            var headers = new Dictionary<string, string>(4);
            if (options == null)
            {
                return headers;
            }

            AddHeaderIfNotEmpty(headers, GameFrameXApiKeyHeader, options.GameFrameXApiKey);
            AddHeaderIfNotEmpty(headers, GameFrameXAppIdHeader, options.GameFrameXAppId);
            AddHeaderIfNotEmpty(headers, GameFrameXAppSecretHeader, options.GameFrameXAppSecret);
            AddHeaderIfNotEmpty(headers, GameFrameXTenantSecretHeader, options.GameFrameXTenantSecret);
            return headers;
        }

        private static void AddHeaderIfNotEmpty(Dictionary<string, string> headers, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                headers[key] = value;
            }
        }

        /// <summary>
        /// 应用运行时默认参数到 HTTP 参数对象。
        /// </summary>
        /// <remarks>
        /// Applies runtime default values to the HTTP parameters, including language, version and device info.
        /// </remarks>
        /// <param name="parameters">HTTP 参数对象 / HTTP parameters object</param>
        private static void ApplyRuntimeDefaults(IStartupHttpParams parameters)
        {
            var startupHttpParams = parameters as StartupHttpParams;
            if (startupHttpParams == null)
            {
                return;
            }

            startupHttpParams.Language = Application.systemLanguage.ToString();
            startupHttpParams.UserLanguage = GameEntry.GetComponent<LocalizationComponent>().Language;
            startupHttpParams.AppVersion = Application.version;
            startupHttpParams.DeviceUniqueIdentifier = SystemInfo.Runtime.BlankDeviceUniqueIdentifier.DeviceUniqueIdentifier;
            startupHttpParams.Platform = ApplicationHelper.PlatformName;
        }

        /// <summary>
        /// 完成启动失败流程，通知 UI 显示错误信息并触发失败事件。
        /// </summary>
        /// <remarks>
        /// Completes the startup failure procedure, notifies UI to display error message and fires failure event.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <param name="procedureName">失败的流程名称 / Failed procedure name</param>
        /// <param name="failedUrl">失败的请求 URL / Failed request URL</param>
        /// <param name="errorMessage">错误信息 / Error message</param>
        public static void CompleteFailure(IFsm<IProcedureManager> procedureOwner, string procedureName, string failedUrl, string errorMessage)
        {
            var result = StartupResult.Fail(procedureName, failedUrl, errorMessage);

            // 失败收尾是挂起兜底的最后出口：任何通知异常都不能阻止 TCS complete，否则 awaiter 永挂。
            try
            {
                GetUIHandler(procedureOwner)?.SetTipText(errorMessage);
                GameFrameworkEntry.GetModule<IEventManager>().Fire(procedureOwner, StartupFailedEventArgs.Create(procedureName, failedUrl, errorMessage));
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }

            TrySetResult(procedureOwner, result);
        }

        private static void TrySetResult(IFsm<IProcedureManager> procedureOwner, StartupResult result)
        {
            UniTaskCompletionSource<StartupResult> completionSource = null;
            try
            {
                completionSource = GetCompletionSource(procedureOwner);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }

            // context 不可用（注入缺失/损坏）时退回 StartupRunner 持有的完成源，保证 awaiter 一定收到结果
            completionSource = completionSource ?? StartupRunner.ActiveCompletionSource;
            completionSource?.TrySetResult(result);
        }

        /// <summary>
        /// 完成启动成功流程，触发成功事件并设置结果。
        /// </summary>
        /// <remarks>
        /// Completes the startup success procedure, fires success event and sets result.
        /// 事件是通知、结果是契约：通知基础设施异常只记录日志，不得把成功结果翻转为失败。
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        public static void CompleteSuccess(IFsm<IProcedureManager> procedureOwner)
        {
            try
            {
                GameFrameworkEntry.GetModule<IEventManager>().Fire(procedureOwner, StartupCompletedEventArgs.Create());
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }

            TrySetResult(procedureOwner, StartupResult.Succeed());
        }

        /// <summary>
        /// 设置下载进度到界面处理器。
        /// </summary>
        /// <remarks>
        /// Sets the download progress to the UI handler with formatted byte sizes.
        /// </remarks>
        /// <param name="uiHandler">界面处理器 / UI handler</param>
        /// <param name="currentBytes">当前已下载字节数 / Current downloaded bytes</param>
        /// <param name="totalBytes">总下载字节数 / Total downloaded bytes</param>
        public static void SetDownloadProgress(IStartupUIHandler uiHandler, long currentBytes, long totalBytes)
        {
            var progress = totalBytes <= 0 ? 0f : currentBytes / (totalBytes * 1f);
            var currentSize = FileHelper.GetBytesSize(currentBytes);
            var totalSize = FileHelper.GetBytesSize(totalBytes);
            uiHandler?.SetProgress(progress, currentSize + "/" + totalSize);
        }
    }
}
