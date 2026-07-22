using System.Collections;
using Cysharp.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 执行资源文件下载流程。启动 YooAsset 下载器进行资源文件下载。
    /// </summary>
    /// <remarks>
    /// Download web files procedure. Launches YooAsset downloader to download resource files.
    /// </remarks>
    internal sealed class ProcedureDownloadWebFiles : ProcedureBase
    {
        /// <inheritdoc />
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.DownloadWebFiles));
            BeginDownload(procedureOwner).ToUniTask();
        }

        /// <summary>
        /// 开始执行资源文件下载。
        /// </summary>
        /// <remarks>
        /// Begins the resource file download process with progress updates and error handling.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>下载完成协程 / Download completion coroutine</returns>
        private IEnumerator BeginDownload(IFsm<IProcedureManager> procedureOwner)
        {
            var downloader = (ResourceDownloaderOperation)procedureOwner.GetData<VarObject>("Downloader").GetValue();

            downloader.OnDownloadErrorCallback = data =>
            {
                GameEntry.GetComponent<EventComponent>().Fire(this, AssetWebFileDownloadFailedEventArgs.Create(data.PackageName, data.FileName, data.ErrorInfo));
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
            };
            downloader.OnDownloadProgressCallback = data =>
            {
                GameEntry.GetComponent<EventComponent>().Fire(this, AssetDownloadProgressUpdateEventArgs.Create(data.PackageName, data.TotalDownloadCount, data.CurrentDownloadCount, data.TotalDownloadBytes, data.CurrentDownloadBytes));
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
