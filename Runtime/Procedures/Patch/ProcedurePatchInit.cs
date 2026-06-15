using Cysharp.Threading.Tasks;

using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    internal sealed class ProcedurePatchInit : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await InitPatchAsync(procedureOwner);
        }

        private async UniTask InitPatchAsync(IFsm<IProcedureManager> procedureOwner)
        {
            if (GameApp.Asset.GamePlayMode == EPlayMode.EditorSimulateMode || GameApp.Asset.GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                await GameApp.Asset.InitPackageAsync(AssetComponent.BuildInPackageName, string.Empty, string.Empty, true);
                ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
                return;
            }

            var packageUrl = procedureOwner.GetData<VarString>(AssetComponent.BuildInPackageName);
            await GameApp.Asset.InitPackageAsync(AssetComponent.BuildInPackageName, packageUrl.Value, packageUrl.Value, true);
            procedureOwner.RemoveData(AssetComponent.BuildInPackageName);
            await UniTask.DelayFrame();
            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }
    }
}
