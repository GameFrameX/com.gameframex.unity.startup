using System.Collections;

using Cysharp.Threading.Tasks;

using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    internal sealed class ProcedureDownloadWebFiles : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameApp.Event.Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.DownloadWebFiles));
            BeginDownload(procedureOwner).ToUniTask();
        }

        private IEnumerator BeginDownload(IFsm<IProcedureManager> procedureOwner)
        {
            var downloader = (ResourceDownloaderOperation)procedureOwner.GetData<VarObject>("Downloader").GetValue();

            downloader.OnDownloadErrorCallback = data =>
            {
                GameApp.Event.Fire(this, AssetWebFileDownloadFailedEventArgs.Create(data.PackageName, data.FileName, data.ErrorInfo));
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
            };
            downloader.OnDownloadProgressCallback = data =>
            {
                GameApp.Event.Fire(this, AssetDownloadProgressUpdateEventArgs.Create(
                    data.PackageName,
                    data.TotalDownloadCount,
                    data.CurrentDownloadCount,
                    data.TotalDownloadBytes,
                    data.CurrentDownloadBytes));
                StartupProcedureUtility.SetDownloadProgress(
                    StartupProcedureUtility.GetUIHandler(procedureOwner),
                    data.CurrentDownloadBytes,
                    data.TotalDownloadBytes);
            };

            downloader.BeginDownload();
            yield return downloader;

            if (downloader.Status == EOperationStatus.Succeed)
            {
                ChangeState<ProcedurePatchDone>(procedureOwner);
            }
        }
    }
}
