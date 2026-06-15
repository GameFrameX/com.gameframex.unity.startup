using Cysharp.Threading.Tasks;

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

namespace GameFrameX.Startup.Runtime
{
    public sealed class ProcedureGameLauncherState : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await LaunchGameAsync(procedureOwner);
        }

        private static async UniTask LaunchGameAsync(IFsm<IProcedureManager> procedureOwner)
        {
            await UniTask.DelayFrame();

            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var hotfixLauncher = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupHotfixLauncher).Value as IHotfixLauncher;
            var result = await hotfixLauncher.StartAsync(options);
            uiHandler?.Dispose();

            if (result.Success)
            {
                StartupProcedureUtility.CompleteSuccess(procedureOwner);
                return;
            }

            StartupProcedureUtility.CompleteFailure(
                procedureOwner,
                nameof(ProcedureGameLauncherState),
                string.Empty,
                result.ErrorMessage);
        }
    }
}
