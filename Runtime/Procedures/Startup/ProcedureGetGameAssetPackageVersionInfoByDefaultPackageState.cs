using System;
using System.IO;

using Cysharp.Threading.Tasks;

using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 获取游戏资源包版本信息流程（使用默认包）。向服务器请求默认游戏资源包的版本信息。
    /// </summary>
    /// <remarks>
    /// Get game asset package version info by default package state procedure. Requests version info of the default game asset package from server.
    /// </remarks>
    public sealed class ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState : ProcedureBase
    {
        /// <inheritdoc />
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await GetGameAssetPackageVersionInfoAsync(procedureOwner);
        }

        /// <summary>
        /// 异步获取游戏资源包版本信息。
        /// </summary>
        /// <remarks>
        /// Asynchronously retrieves game asset package version info with retry support. Constructs package URL from response and stores it in procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>获取完成的协程 / Retrieval completion coroutine</returns>
        private async UniTask GetGameAssetPackageVersionInfoAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var httpParamsProvider = StartupProcedureUtility.GetHttpParamsProvider(procedureOwner);
            var jsonParams = StartupProcedureUtility.CreateHttpParams(options, httpParamsProvider);
            jsonParams["AssetPackageName"] = AssetComponent.BuildInPackageName;

            for (var retryIndex = 1; retryIndex <= options.MaxAttemptsPerUrl; retryIndex++)
            {
                try
                {
                    var json = await GameApp.Web.PostToString(GameApp.GlobalConfig.CheckResourceVersionUrl, jsonParams);
                    var httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json.Result);
                    if (httpJsonResult.Code <= 0)
                    {
                        var packageVersion = Utility.Json.ToObject<ResponseGameAssetPackageVersion>(httpJsonResult.Data);
                        var packageUrl = Path.Combine(
                            packageVersion.RootPath,
                            packageVersion.PackageName,
                            packageVersion.Platform,
                            packageVersion.AppVersion,
                            packageVersion.Channel,
                            packageVersion.AssetPackageName,
                            packageVersion.Version) + Path.DirectorySeparatorChar;

                        var urlValue = ReferencePool.Acquire<VarString>();
                        urlValue.SetValue(packageUrl);
                        procedureOwner.SetData(AssetComponent.BuildInPackageName, urlValue);

                        var versionValue = ReferencePool.Acquire<VarString>();
                        versionValue.SetValue(packageVersion.Version);
                        procedureOwner.SetData(AssetComponent.BuildInPackageName + "Version", versionValue);

                        ChangeState<ProcedurePatchInit>(procedureOwner);
                        return;
                    }

                    Log.Error("Get asset package version returned code " + httpJsonResult.Code);
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }

                if (retryIndex >= options.MaxAttemptsPerUrl)
                {
                    StartupProcedureUtility.CompleteFailure(
                        procedureOwner,
                        nameof(ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState),
                        GameApp.GlobalConfig.CheckResourceVersionUrl,
                        "Failed to get asset package version info.");
                    return;
                }

                uiHandler?.SetTipText("Getting asset version failed, retrying... (" + retryIndex + "/" + options.MaxAttemptsPerUrl + ")");
                await UniTask.Delay(options.RetryDelayMs);
            }
        }
    }
}
