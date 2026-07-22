using Cysharp.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 资源包初始化流程。根据游戏运行模式初始化 YooAsset 资源包。
    /// </summary>
    /// <remarks>
    /// Patch initialization procedure. Initializes YooAsset package based on game play mode.
    /// </remarks>
    internal sealed class ProcedurePatchInit : ProcedureBase
    {
        /// <inheritdoc />
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await InitPatchAsync(procedureOwner);
        }

        /// <summary>
        /// 异步执行资源包初始化。
        /// </summary>
        /// <remarks>
        /// Asynchronously initializes the asset package. In simulate or offline mode, uses empty URLs.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>初始化完成的协程 / Initialization completion coroutine</returns>
        private async UniTask InitPatchAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var assetComponent = GameEntry.GetComponent<AssetComponent>();
            if (assetComponent.GamePlayMode == EPlayMode.EditorSimulateMode || assetComponent.GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                await assetComponent.InitPackageAsync(AssetComponent.BuildInPackageName, string.Empty, string.Empty, true);
                ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
                return;
            }

            var packageUrl = procedureOwner.GetData<VarString>(AssetComponent.BuildInPackageName);
            await assetComponent.InitPackageAsync(AssetComponent.BuildInPackageName, packageUrl.Value, packageUrl.Value, true);
            procedureOwner.RemoveData(AssetComponent.BuildInPackageName);
            await UniTask.DelayFrame();
            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }
    }
}