using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using GameFrameX.Fsm.Runtime;
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
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupOptions).Value as StartupOptions;
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
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupUIHandler).Value as IStartupUIHandler;
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
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupCompletionSource).Value as UniTaskCompletionSource<StartupResult>;
        }

        /// <summary>
        /// 从流程所有者中获取 HTTP 参数提供者。
        /// </summary>
        /// <remarks>
        /// Gets the HTTP params provider from the procedure owner. Returns default provider if not set.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>HTTP 参数提供者实例 / HTTP params provider instance</returns>
        public static IStartupHttpParamsProvider GetHttpParamsProvider(IFsm<IProcedureManager> procedureOwner)
        {
            var providerBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupHttpParamsProvider);
            return providerBox?.Value as IStartupHttpParamsProvider ?? new DefaultStartupHttpParamsProvider();
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
            startupHttpParams.UserLanguage = GameApp.Localization.Language;
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
            GetUIHandler(procedureOwner)?.SetTipText(errorMessage);
            GameApp.Event.Fire(procedureOwner, StartupFailedEventArgs.Create(procedureName, failedUrl, errorMessage));
            GetCompletionSource(procedureOwner)?.TrySetResult(result);
        }

        /// <summary>
        /// 完成启动成功流程，触发成功事件并设置结果。
        /// </summary>
        /// <remarks>
        /// Completes the startup success procedure, fires success event and sets result.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        public static void CompleteSuccess(IFsm<IProcedureManager> procedureOwner)
        {
            GameApp.Event.Fire(procedureOwner, StartupCompletedEventArgs.Create());
            GetCompletionSource(procedureOwner)?.TrySetResult(StartupResult.Succeed());
        }

        /// <summary>
        /// 设置下载进度到界面处理器。
        /// </summary>
        /// <remarks>
        /// Sets the download progress to the UI handler with formatted byte sizes.
        /// </remarks>
        /// <param name="uiHandler">界面处理器 / UI handler</param>
        /// <param name="currentBytes">当前已下载字节数 / Current downloaded bytes</param>
        /// <param name="totalBytes">总字节数 / Total bytes</param>
        public static void SetDownloadProgress(IStartupUIHandler uiHandler, long currentBytes, long totalBytes)
        {
            var progress = totalBytes <= 0 ? 0f : currentBytes / (totalBytes * 1f);
            var currentSize = Utility.File.GetBytesSize(currentBytes);
            var totalSize = Utility.File.GetBytesSize(totalBytes);
            uiHandler?.SetProgress(progress, currentSize + "/" + totalSize);
        }
    }
}
