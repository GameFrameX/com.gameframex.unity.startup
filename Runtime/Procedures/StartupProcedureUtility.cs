using System.Collections.Generic;

using Cysharp.Threading.Tasks;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.Startup.Runtime
{
    internal static class StartupProcedureUtility
    {
        public static StartupOptions GetOptions(IFsm<IProcedureManager> procedureOwner)
        {
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupOptions).Value as StartupOptions;
        }

        public static IStartupUIHandler GetUIHandler(IFsm<IProcedureManager> procedureOwner)
        {
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupUIHandler).Value as IStartupUIHandler;
        }

        public static UniTaskCompletionSource<StartupResult> GetCompletionSource(IFsm<IProcedureManager> procedureOwner)
        {
            return procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupCompletionSource).Value as UniTaskCompletionSource<StartupResult>;
        }

        public static IStartupHttpParamsProvider GetHttpParamsProvider(IFsm<IProcedureManager> procedureOwner)
        {
            var providerBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupHttpParamsProvider);
            return providerBox?.Value as IStartupHttpParamsProvider ?? new DefaultStartupHttpParamsProvider();
        }

        public static Dictionary<string, object> CreateHttpParams(StartupOptions options)
        {
            return CreateHttpParams(options, new DefaultStartupHttpParamsProvider());
        }

        public static Dictionary<string, object> CreateHttpParams(StartupOptions options, IStartupHttpParamsProvider provider)
        {
            var parameters = provider.Create(options);
            ApplyRuntimeDefaults(parameters);
            return parameters.ToDictionary();
        }

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

        public static void CompleteFailure(IFsm<IProcedureManager> procedureOwner, string procedureName, string failedUrl, string errorMessage)
        {
            var result = StartupResult.Fail(procedureName, failedUrl, errorMessage);
            GetUIHandler(procedureOwner)?.SetTipText(errorMessage);
            GameApp.Event.Fire(procedureOwner, StartupFailedEventArgs.Create(procedureName, failedUrl, errorMessage));
            GetCompletionSource(procedureOwner)?.TrySetResult(result);
        }

        public static void CompleteSuccess(IFsm<IProcedureManager> procedureOwner)
        {
            GameApp.Event.Fire(procedureOwner, StartupCompletedEventArgs.Create());
            GetCompletionSource(procedureOwner)?.TrySetResult(StartupResult.Succeed());
        }

        public static void SetDownloadProgress(IStartupUIHandler uiHandler, long currentBytes, long totalBytes)
        {
            var progress = totalBytes <= 0 ? 0f : currentBytes / (totalBytes * 1f);
            var currentSize = Utility.File.GetBytesSize(currentBytes);
            var totalSize = Utility.File.GetBytesSize(totalBytes);
            uiHandler?.SetProgress(progress, currentSize + "/" + totalSize);
        }
    }
}
