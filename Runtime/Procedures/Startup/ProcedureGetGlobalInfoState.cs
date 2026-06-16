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
    /// <summary>
    /// 获取全局配置信息流程。使用 URL 故障转移机制从服务器获取全局配置信息。
    /// </summary>
    /// <remarks>
    /// Get global info state procedure. Retrieves global configuration from server using URL failover mechanism.
    /// </remarks>
    public sealed class ProcedureGetGlobalInfoState : ProcedureBase
    {
        /// <inheritdoc />
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

        /// <summary>
        /// 异步获取全局配置信息。
        /// </summary>
        /// <remarks>
        /// Asynchronously retrieves global configuration using URL failover runner. Updates global config on success.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>获取完成的协程 / Retrieval completion coroutine</returns>
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