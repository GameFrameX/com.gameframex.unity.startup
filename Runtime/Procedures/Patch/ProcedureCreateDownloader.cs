using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 创建资源下载器流程。创建 YooAsset 下载器并检查是否有资源需要下载。
    /// </summary>
    /// <remarks>
    /// Create resource downloader procedure. Creates YooAsset downloader and checks if any resources need to be downloaded.
    /// </remarks>
    internal sealed class ProcedureCreateDownloader : ProcedureBase
    {
        /// <inheritdoc />
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameApp.Event.Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.CreateDownloader));
            CreateDownloader(procedureOwner);
        }

        /// <summary>
        /// 创建资源下载器实例。
        /// </summary>
        /// <remarks>
        /// Creates a resource downloader instance and stores it in procedure owner data.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        private void CreateDownloader(IFsm<IProcedureManager> procedureOwner)
        {
            var downloader = YooAssets.CreateResourceDownloader(10, 3);
            var downloaderValue = ReferencePool.Acquire<VarObject>();
            downloaderValue.SetValue(downloader);
            procedureOwner.SetData("Downloader", downloaderValue);

            if (downloader.TotalDownloadCount == 0)
            {
                Debug.Log("No resources need to be downloaded.");
                ChangeState<ProcedurePatchDone>(procedureOwner);
                return;
            }

            GameApp.Event.Fire(this, AssetFoundUpdateFilesEventArgs.Create(
                downloader.GetPackageName(),
                downloader.TotalDownloadCount,
                downloader.TotalDownloadBytes));
            ChangeState<ProcedureDownloadWebFiles>(procedureOwner);
        }
    }
}
