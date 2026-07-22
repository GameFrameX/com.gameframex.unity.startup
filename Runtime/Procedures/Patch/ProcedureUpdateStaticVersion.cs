using System.Collections;

using Cysharp.Threading.Tasks;

using GameFrameX.Asset.Runtime;
using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 更新静态版本流程。从 YooAsset 获取资源包的最新版本信息。
    /// </summary>
    /// <remarks>
    /// Update static version procedure. Requests the latest version information of the asset package from YooAsset.
    /// </remarks>
    internal sealed class ProcedureUpdateStaticVersion : ProcedureBase
    {
        /// <inheritdoc />
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.UpdateStaticVersion));
            GetStaticVersion(procedureOwner).ToUniTask();
        }

        /// <summary>
        /// 获取静态版本信息。
        /// </summary>
        /// <remarks>
        /// Requests the static package version from YooAsset and stores it in offline mode.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>版本获取协程 / Version request coroutine</returns>
        private IEnumerator GetStaticVersion(IFsm<IProcedureManager> procedureOwner)
        {
            var package = YooAssets.GetPackage(AssetComponent.BuildInPackageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                var assetComponent = GameEntry.GetComponent<AssetComponent>();
                if (assetComponent.GamePlayMode == EPlayMode.OfflinePlayMode)
                {
                    var versionValue = ReferencePool.Acquire<VarString>();
                    versionValue.SetValue(operation.PackageVersion);
                    procedureOwner.SetData(AssetComponent.BuildInPackageName + "Version", versionValue);
                }

                Debug.Log("Updated package Version : " + operation.PackageVersion);
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
                yield break;
            }

            Debug.LogError(operation.Error);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetStaticVersionUpdateFailedEventArgs.Create(AssetComponent.BuildInPackageName, operation.Error));
            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }
    }
}
