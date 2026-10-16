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
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await StartupProcedureUtility.RunGuardedAsync(procedureOwner, nameof(ProcedureUpdateStaticVersion), () =>
            {
                GameFrameworkEntry.GetModule<IEventManager>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.UpdateStaticVersion));
                return GetStaticVersion(procedureOwner).ToUniTask();
            });
        }

        /// <summary>
        /// 获取静态版本信息。
        /// </summary>
        /// <remarks>
        /// Requests the static package version from YooAsset and stores it in offline mode.
        /// In host play mode, falls back to the local version (sandbox record or buildin) when the remote version is unreachable.
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

                // 清理可能残留的本地回退标记
                procedureOwner.RemoveData(AssetComponent.BuildInPackageName + "VersionFromLocal");

                Debug.Log("Updated package Version : " + operation.PackageVersion);
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
                yield break;
            }

            // 远端版本不可达时，Host 模式回退到本地版本（沙盒记录 -> 包内版本），支持断网启动
            if (GameEntry.GetComponent<AssetComponent>().GamePlayMode == EPlayMode.HostPlayMode)
            {
                var localVersionOperation = package.RequestLocalVersionAsync(false);
                yield return localVersionOperation;

                if (localVersionOperation.Status == EOperationStatus.Succeed)
                {
                    Debug.LogWarning("Request remote package version failed, using local version : " + localVersionOperation.PackageVersion);
                    StartupProcedureUtility.GetUIHandler(procedureOwner)?.SetTipText("Using local asset version...");

                    procedureOwner.RemoveData(AssetComponent.BuildInPackageName + "Version");
                    var versionValue = ReferencePool.Acquire<VarString>();
                    versionValue.SetValue(localVersionOperation.PackageVersion);
                    procedureOwner.SetData(AssetComponent.BuildInPackageName + "Version", versionValue);

                    var versionFromLocalValue = ReferencePool.Acquire<VarBoolean>();
                    versionFromLocalValue.SetValue(true);
                    procedureOwner.SetData(AssetComponent.BuildInPackageName + "VersionFromLocal", versionFromLocalValue);

                    ChangeState<ProcedureUpdateManifest>(procedureOwner);
                    yield break;
                }
            }

            Debug.LogError(operation.Error);
            GameFrameworkEntry.GetModule<IEventManager>().Fire(this, AssetStaticVersionUpdateFailedEventArgs.Create(AssetComponent.BuildInPackageName, operation.Error));
            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }
    }
}
