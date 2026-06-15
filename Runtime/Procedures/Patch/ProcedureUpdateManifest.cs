using System.Collections;

using Cysharp.Threading.Tasks;

using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    internal sealed class ProcedureUpdateManifest : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await UpdateManifestAsync(procedureOwner);
        }

        private async UniTask UpdateManifestAsync(IFsm<IProcedureManager> procedureOwner)
        {
            if (GameApp.Asset.GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                var versionValue = procedureOwner.GetData<VarString>(AssetComponent.BuildInPackageName + "Version");
                var package = GameApp.Asset.GetAssetsPackage(AssetComponent.BuildInPackageName);
                var operation = package.UpdatePackageManifestAsync(versionValue.Value);
                await operation.ToUniTask();
                ChangeState<ProcedurePatchDone>(procedureOwner);
                return;
            }

            GameApp.Event.Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.UpdateManifest));
            await UpdateManifest(procedureOwner).ToUniTask();
        }

        private IEnumerator UpdateManifest(IFsm<IProcedureManager> procedureOwner)
        {
            yield return new WaitForSecondsRealtime(0.1f);

            var package = YooAssets.GetPackage(AssetComponent.BuildInPackageName);
            UpdatePackageManifestOperation operation;
            if (GameApp.Asset.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                operation = package.UpdatePackageManifestAsync("Simulate");
            }
            else
            {
                var versionValue = procedureOwner.GetData<VarString>(AssetComponent.BuildInPackageName + "Version");
                operation = package.UpdatePackageManifestAsync(versionValue.Value);
            }

            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                procedureOwner.RemoveData(AssetComponent.BuildInPackageName + "Version");
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
                yield break;
            }

            Debug.LogError(operation.Error);
            GameApp.Event.Fire(this, AssetPatchManifestUpdateFailedEventArgs.Create(AssetComponent.BuildInPackageName, operation.Error));
            ChangeState<ProcedureUpdateManifest>(procedureOwner);
        }
    }
}
