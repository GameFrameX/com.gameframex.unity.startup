using System;

using Cysharp.Threading.Tasks;

using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using GameFrameX.Web.Runtime;

using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    public sealed class ProcedureGetAppVersionInfoState : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            if (GameApp.Asset.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Debug.Log("Editor simulate mode, skip app version request.");
                ChangeState<ProcedurePatchInit>(procedureOwner);
                return;
            }

            await GetAppVersionInfoAsync(procedureOwner);
        }

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
                    var json = await GameApp.Web.PostToString(GameApp.GlobalConfig.CheckAppVersionUrl, jsonParams);
                    var httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json.Result);
                    if (httpJsonResult.Code <= 0)
                    {
                        var gameAppVersion = Utility.Json.ToObject<ResponseGameAppVersion>(httpJsonResult.Data);
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
                    StartupProcedureUtility.CompleteFailure(
                        procedureOwner,
                        nameof(ProcedureGetAppVersionInfoState),
                        GameApp.GlobalConfig.CheckAppVersionUrl,
                        "Failed to get app version info.");
                    return;
                }

                uiHandler?.SetTipText("Server error, retrying... (" + retryIndex + "/" + options.MaxAttemptsPerUrl + ")");
                await UniTask.Delay(options.RetryDelayMs);
            }
        }
    }
}
