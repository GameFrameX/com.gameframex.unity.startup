using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

using UnityEngine;
using YooAsset;

namespace GameFrameX.Startup.Runtime
{
    internal sealed class ProcedureCreateDownloader : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameApp.Event.Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.CreateDownloader));
            CreateDownloader(procedureOwner);
        }

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
