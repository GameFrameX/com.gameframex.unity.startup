using Cysharp.Threading.Tasks;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动 UI 处理接口，由应用层实现。包内不依赖具体 UI 后端（FairyGUI / UGUI / 自定义）。
    /// </summary>
    public interface IStartupUIHandler
    {
        /// <summary>
        /// 打开启动 UI（全屏），完成 UI 资源加载。
        /// </summary>
        /// <param name="uiResName">UI 资源路径（来自 StartupOptions.LauncherUIResName）。</param>
        UniTask StartAsync(string uiResName);

        /// <summary>
        /// 更新启动 UI 上的提示文本。
        /// </summary>
        void SetTipText(string text);

        /// <summary>
        /// 更新下载进度条和文本。
        /// </summary>
        /// <param name="progress">0-1 范围的进度值。</param>
        /// <param name="sizeInfo">如 "5.2MB / 100MB"。</param>
        void SetProgress(float progress, string sizeInfo);

        /// <summary>
        /// 标记下载完成状态。
        /// </summary>
        void SetProgressUpdateFinish();

        /// <summary>
        /// 显示应用版本升级弹窗。
        /// </summary>
        /// <returns>true 表示继续启动流程；false 表示流程停留在升级弹窗或外部下载页。</returns>
        UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo);

        /// <summary>
        /// 关闭启动 UI、释放订阅。
        /// </summary>
        void Dispose();
    }
}
