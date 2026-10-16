using Cysharp.Threading.Tasks;

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

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
            await StartupProcedureUtility.RunGuardedAsync(procedureOwner, nameof(ProcedureGameLauncherState), () => LaunchGameAsync(procedureOwner));
        }

        /// <summary>
        /// 异步执行游戏启动。
        /// </summary>
        /// <remarks>
        /// Asynchronously launches the game by calling the hotfix launcher. On success, completes startup successfully;
        /// otherwise completes with failure and error message. The startup UI is disposed only on success — on failure
        /// it stays alive to display the error message, and its lifetime is handed back to the caller.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>游戏启动协程 / Game launch coroutine</returns>
        private static async UniTask LaunchGameAsync(IFsm<IProcedureManager> procedureOwner)
        {
            await UniTask.DelayFrame();

            var context = StartupProcedureUtility.GetContext(procedureOwner);
            var result = await context.HotfixLauncher.StartAsync(context.Options);

            if (result.Success)
            {
                context.UIHandler.Dispose();
                StartupProcedureUtility.CompleteSuccess(procedureOwner);
                return;
            }

            // 失败时保留启动 UI 展示错误信息；UI 生命周期交还调用方决定（重试 / 退出）。
            StartupProcedureUtility.CompleteFailure(
                procedureOwner,
                nameof(ProcedureGameLauncherState),
                string.Empty,
                result.ErrorMessage);
        }
    }
}
