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
    public sealed class ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await GetGameAssetPackageVersionInfoAsync(procedureOwner);
        }

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
