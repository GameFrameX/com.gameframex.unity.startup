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
    /// <summary>
    /// 更新资源清单流程。根据版本信息更新 YooAsset 资源清单。
    /// </summary>
    /// <remarks>
    /// Update manifest procedure. Updates YooAsset package manifest based on version information.
    /// </remarks>
    internal sealed class ProcedureUpdateManifest : ProcedureBase
    {
        /// <inheritdoc />
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await UpdateManifestAsync(procedureOwner);
        }

        /// <summary>
        /// 异步执行清单更新。
        /// </summary>
        /// <remarks>
        /// Asynchronously updates the package manifest. In offline mode, proceeds directly to patch done.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>更新完成的协程 / Update completion coroutine</returns>
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

        /// <summary>
        /// 执行资源清单更新协程。
        /// </summary>
        /// <remarks>
        /// Executes the package manifest update coroutine with retry on failure.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>清单更新协程 / Manifest update coroutine</returns>
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
