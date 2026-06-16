using Cysharp.Threading.Tasks;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 热更启动接口，由应用层实现。包内不绑定具体热更方案（HybridCLR / 其他）。
    /// </summary>
    public interface IHotfixLauncher
    {
        /// <summary>
        /// 加载并启动热更程序集。
        /// </summary>
        /// <param name="options">配置资产，提供 HotfixAssemblyName / HotfixEntryTypeName / HotfixEntryMethodName。</param>
        /// <returns>热更加载结果（成功或失败均返回，不抛异常）。</returns>
        UniTask<HotfixLaunchResult> StartAsync(StartupOptions options);
    }
}
