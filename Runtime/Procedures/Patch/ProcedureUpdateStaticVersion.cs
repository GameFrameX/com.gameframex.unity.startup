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
    internal sealed class ProcedureUpdateStaticVersion : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameApp.Event.Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.UpdateStaticVersion));
            GetStaticVersion(procedureOwner).ToUniTask();
        }

        private IEnumerator GetStaticVersion(IFsm<IProcedureManager> procedureOwner)
        {
            var package = YooAssets.GetPackage(AssetComponent.BuildInPackageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                if (GameApp.Asset.GamePlayMode == EPlayMode.OfflinePlayMode)
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
            GameApp.Event.Fire(this, AssetStaticVersionUpdateFailedEventArgs.Create(AssetComponent.BuildInPackageName, operation.Error));
            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }
    }
}
