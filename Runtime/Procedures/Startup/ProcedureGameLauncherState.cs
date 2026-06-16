using Cysharp.Threading.Tasks;

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 游戏启动器状态流程。执行热更新启动器并等待游戏初始化完成。
    /// </summary>
    /// <remarks>
    /// Game launcher state procedure. Executes the hotfix launcher and waits for game initialization to complete.
    /// </remarks>
    public sealed class ProcedureGameLauncherState : ProcedureBase
    {
        /// <inheritdoc />
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await LaunchGameAsync(procedureOwner);
        }

        /// <summary>
        /// 异步执行游戏启动。
        /// </summary>
        /// <remarks>
        /// Asynchronously launches the game by calling the hotfix launcher. On success, completes startup successfully;
        /// otherwise completes with failure and error message.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>游戏启动协程 / Game launch coroutine</returns>
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
