using System;
using Cysharp.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;
using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 获取 App 版本信息流程。向服务器请求 App 版本信息并检查是否需要升级。
    /// </summary>
    /// <remarks>
    /// Get app version info state procedure. Requests app version info from server and checks if upgrade is required.
    /// </remarks>
    public sealed class ProcedureGetAppVersionInfoState : ProcedureBase
    {
        /// <inheritdoc />
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            var assetComponent = GameEntry.GetComponent<AssetComponent>();
            if (assetComponent.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Debug.Log("Editor simulate mode, skip app version request.");
                ChangeState<ProcedurePatchInit>(procedureOwner);
                return;
            }

            await GetAppVersionInfoAsync(procedureOwner);
        }

        /// <summary>
        /// 异步获取 App 版本信息。
        /// </summary>
        /// <remarks>
        /// Asynchronously retrieves app version info from server with retry support. Shows upgrade dialog if update is available.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>获取完成的协程 / Retrieval completion coroutine</returns>
        private async UniTask GetAppVersionInfoAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var httpParamsProvider = StartupProcedureUtility.GetHttpParamsProvider(procedureOwner);
            var jsonParams = StartupProcedureUtility.CreateHttpParams(options, httpParamsProvider);

            for (var retryIndex = 1; retryIndex <= options.MaxAttemptsPerUrl; retryIndex++)
            {
                try
                {
                    var json = await GameEntry.GetComponent<WebComponent>().PostToString(GameEntry.GetComponent<GlobalConfigComponent>().CheckAppVersionUrl, jsonParams);
                    var httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json.Result);
                    if (httpJsonResult.Code <= 0)
                    {
                        var gameAppVersion = Utility.Json.ToObject<ResponseGameAppVersion>(httpJsonResult.Data);
                        StartupNetworkCacheUtility.SaveAppVersionInfo(httpJsonResult.Data);
                        await ApplyAppVersionInfoAsync(procedureOwner, uiHandler, gameAppVersion);
                        return;
                    }

                    Log.Error("Get app version returned code " + httpJsonResult.Code);
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }

                if (retryIndex >= options.MaxAttemptsPerUrl)
                {
                    if (StartupNetworkCacheUtility.TryGetCachedAppVersionInfo(out var cachedGameAppVersion))
                    {
                        uiHandler?.SetTipText("Using cached app version...");
                        await ApplyAppVersionInfoAsync(procedureOwner, uiHandler, cachedGameAppVersion);
                        return;
                    }

                    StartupProcedureUtility.CompleteFailure(procedureOwner, nameof(ProcedureGetAppVersionInfoState), GameEntry.GetComponent<GlobalConfigComponent>().CheckAppVersionUrl, "Failed to get app version info.");
                    return;
                }

                uiHandler?.SetTipText("Server error, retrying... (" + retryIndex + "/" + options.MaxAttemptsPerUrl + ")");
                await UniTask.Delay(options.RetryDelayMs);
            }
        }

        private async UniTask ApplyAppVersionInfoAsync(
            IFsm<IProcedureManager> procedureOwner,
            IStartupUIHandler uiHandler,
            ResponseGameAppVersion gameAppVersion)
        {
            if (gameAppVersion.IsUpgrade)
            {
                var shouldContinue = await uiHandler.ShowUpgradeAsync(new StartupUpgradeInfo(
                                                                          gameAppVersion.IsForce,
                                                                          gameAppVersion.AppDownloadUrl,
                                                                          gameAppVersion.UpdateTitle,
                                                                          gameAppVersion.UpdateAnnouncement));

                if (!shouldContinue)
                {
                    return;
                }
            }

            ChangeState<ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState>(procedureOwner);
        }
    }
}