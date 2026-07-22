using Cysharp.Threading.Tasks;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动入口状态。负责拉起启动 UI，然后切换到下一个状态。
    /// </summary>
    /// <remarks>
    /// Launcher entry state. Responsible for launching the startup UI, then transitions to the next state.
    /// </remarks>
    public sealed class ProcedureLauncherState : ProcedureBase
    {
        /// <inheritdoc />
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            await StartLauncherUIAsync(procedureOwner);
        }

        private async UniTask StartLauncherUIAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var optionsBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupOptions);
            var uiHandlerBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupUIHandler);

            var options = optionsBox.Value as StartupOptions;
            var uiHandler = uiHandlerBox.Value as IStartupUIHandler;

            if (options == null || uiHandler == null)
            {
                return;
            }

            var webComponent = GameEntry.GetComponent<WebComponent>();
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXApiKeyHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXAppIdHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXAppSecretHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXTenantSecretHeader);

            foreach (var header in StartupProcedureUtility.CreateGameFrameXHeaders(options))
            {
                webComponent.AddBaseHeader(header.Key, header.Value);
            }

            await uiHandler.StartAsync(options.LauncherUIResName);

            await UniTask.NextFrame();
            ChangeState<ProcedureGetGlobalInfoState>(procedureOwner);
        }
    }
}