using GameFrameX.Asset.Runtime;
using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 资源包补丁流程完成流程。更新界面状态并切换到游戏启动流程。
    /// </summary>
    /// <remarks>
    /// Patch done procedure. Updates UI state and transitions to game launcher procedure.
    /// </remarks>
    internal sealed class ProcedurePatchDone : ProcedureBase
    {
        /// <inheritdoc />
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.PatchDone));
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            uiHandler?.SetProgressUpdateFinish();
            uiHandler?.SetTipText(string.Empty);
            Debug.Log("Patch flow completed.");
            ChangeState<ProcedureGameLauncherState>(procedureOwner);
        }
    }
}
