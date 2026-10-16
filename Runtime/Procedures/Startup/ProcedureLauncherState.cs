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
            await StartupProcedureUtility.RunGuardedAsync(procedureOwner, nameof(ProcedureLauncherState), () => StartLauncherUIAsync(procedureOwner));
        }

        private async UniTask StartLauncherUIAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var context = StartupProcedureUtility.GetContext(procedureOwner);
            var options = context.Options;
            var uiHandler = context.UIHandler;

            var webComponent = GameEntry.GetComponent<WebComponent>();
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXApiKeyHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXAppIdHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXAppSecretHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXTenantSecretHeader);

            foreach (var header in StartupProcedureUtility.CreateGameFrameXHeaders(options))
            {
                webComponent.AddBaseHeader(header.Key, header.Value);
            }

            // iOS 首次联网授权门禁：放行后再进入首个业务请求，确保获取全局信息时必然有网络
            await StartupNetworkGate.WaitForIOSNetworkAsync(options, uiHandler);

            await uiHandler.StartAsync(options.LauncherUIResName);

            await UniTask.NextFrame();
            ChangeState<ProcedureGetGlobalInfoState>(procedureOwner);
        }
    }
}