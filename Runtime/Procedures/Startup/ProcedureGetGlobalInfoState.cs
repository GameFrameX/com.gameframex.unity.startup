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
    public sealed class ProcedureGetGlobalInfoState : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            if (GameApp.Asset.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Debug.Log("Editor simulate mode, skip global info request.");
                ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
                return;
            }

            if (GameApp.Asset.GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                Debug.Log("Offline play mode, skip remote startup requests.");
                ChangeState<ProcedurePatchInit>(procedureOwner);
                return;
            }

            await GetGlobalInfoAsync(procedureOwner);
        }

        private async UniTask GetGlobalInfoAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var httpParamsProvider = StartupProcedureUtility.GetHttpParamsProvider(procedureOwner);
            var jsonParams = StartupProcedureUtility.CreateHttpParams(options, httpParamsProvider);
            var globalConfig = GameApp.GlobalConfig;
            var result = await UrlFailoverRunner.ExecuteAsync(
                             options.GlobalInfoUrls,
                             options.MaxAttemptsPerUrl,
                             options.RetryDelayMs,
                             async url =>
                             {
                                 try
                                 {
                                     var json = await GameApp.Web.PostToString(url, jsonParams);
                                     var responseGlobalInfo = json.Result.ToHttpJsonResultData<ResponseGlobalInfo>();
                                     globalConfig.SetOriginalData(json.Result);
                                     if (!responseGlobalInfo.IsSuccess)
                                     {
                                         return UrlAttemptResult.Fail("Global info server returned code " + responseGlobalInfo.Code);
                                     }

                                     globalConfig.CheckAppVersionUrl = responseGlobalInfo.Data.CheckAppVersionUrl;
                                     globalConfig.CheckResourceVersionUrl = responseGlobalInfo.Data.CheckResourceVersionUrl;
                                     globalConfig.Content = responseGlobalInfo.Data.Content;
                                     globalConfig.SetGlobalConfig(responseGlobalInfo.Data);
                                     return UrlAttemptResult.Succeed();
                                 }
                                 catch (Exception exception)
                                 {
                                     Log.Error(exception);
                                     return UrlAttemptResult.Fail(exception.Message);
                                 }
                             },
                             (url, attempt, total) => uiHandler?.SetTipText("Loading... (" + attempt + "/" + total + ")"));

            if (!result.Success)
            {
                StartupProcedureUtility.CompleteFailure(
                    procedureOwner,
                    nameof(ProcedureGetGlobalInfoState),
                    result.FailedUrl,
                    result.ErrorMessage);
                return;
            }


            uiHandler?.SetTipText("Loading...");
            ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
        }
    }
}