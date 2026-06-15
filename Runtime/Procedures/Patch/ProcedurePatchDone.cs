using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using UnityEngine;

namespace GameFrameX.Startup.Runtime
{
    internal sealed class ProcedurePatchDone : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameApp.Event.Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.PatchDone));
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            uiHandler?.SetProgressUpdateFinish();
            uiHandler?.SetTipText(string.Empty);
            Debug.Log("Patch flow completed.");
            ChangeState<ProcedureGameLauncherState>(procedureOwner);
        }
    }
}
