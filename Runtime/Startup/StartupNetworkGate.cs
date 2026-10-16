using System.Runtime.InteropServices;

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动网络门禁。用于吸收 iOS 首次联网授权期间的首包失败，确保放行后的首个业务请求（全局信息等）必然有网络。
    /// </summary>
    /// <remarks>
    /// iOS 首次启动时，系统会拦截第一个网络请求以弹出"允许使用网络"授权。这里发一个无关紧要的小请求触发授权，
    /// 并循环探测直到网络可用后才放行：
    /// - 探测成功（含"仅 WLAN"用户）→ 放行
    /// - 探测失败且授权状态为受限 → 显示引导持续等待，用户可在系统弹窗点"允许"或去系统设置开启
    /// - 探测失败但已授权（网络故障）→ 有限重试后放行，由后续流程自身的重试兜底
    /// 授权状态由 com.gameframex.unity 包的 GameFrameXNetworkPermission.mm 原生插件提供。
    /// </remarks>
    internal static class StartupNetworkGate
    {
        /// <summary>
        /// 国行 iOS 无线数据授权状态：受限（未授权/用户拒绝/仅 WLAN），定义于 GameFrameXNetworkPermission.mm。
        /// </summary>
        private const int NetworkPermissionStatusRestricted = 1;

        /// <summary>
        /// 等待 iOS 网络就绪。非 iOS 平台或未配置探测 URL 时立即返回。
        /// </summary>
        /// <param name="options">启动配置。</param>
        /// <param name="uiHandler">UI 处理实现，用于展示等待与引导文案。</param>
        /// <returns>网络就绪（或按策略放行）后完成。</returns>
        public static UniTask WaitForIOSNetworkAsync(StartupOptions options, IStartupUIHandler uiHandler)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (options == null || string.IsNullOrWhiteSpace(options.IOSNetworkProbeUrl))
            {
                return UniTask.CompletedTask;
            }

            return WaitForIOSNetworkInternalAsync(options, uiHandler);
#else
            return UniTask.CompletedTask;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        private static async UniTask WaitForIOSNetworkInternalAsync(StartupOptions options, IStartupUIHandler uiHandler)
        {
            var maxFailCount = Mathf.Max(1, options.IOSNetworkProbeMaxFailCount);
            var networkFailCount = 0;

            while (true)
            {
                if (await ProbeNetworkAsync(options))
                {
                    return;
                }

                if (GetNetworkPermissionStatus() == NetworkPermissionStatusRestricted)
                {
                    // 未授权（或仅 WLAN）：持续等待用户在系统弹窗点"允许"，或前往 设置 → 无线数据 开启
                    uiHandler?.SetTipText("正在等待网络授权，请点击系统弹窗中的\"允许\"，或前往 设置 → 无线数据 开启网络权限...");
                }
                else
                {
                    // 已授权/未知但探测失败（网络故障）：有限重试后放行，由后续流程自身的重试兜底
                    networkFailCount++;
                    if (networkFailCount >= maxFailCount)
                    {
                        uiHandler?.SetTipText("Network unavailable, continuing...");
                        return;
                    }

                    uiHandler?.SetTipText("Waiting for network... (" + networkFailCount + "/" + maxFailCount + ")");
                }

                await UniTask.Delay(Mathf.Max(300, options.IOSNetworkProbeRetryDelayMs));
            }
        }

        /// <summary>
        /// 无关紧要的探测请求，仅用于触发系统授权弹窗与判定网络是否可用。
        /// </summary>
        private static async UniTask<bool> ProbeNetworkAsync(StartupOptions options)
        {
            using (var request = UnityWebRequest.Get(options.IOSNetworkProbeUrl))
            {
                // 超时需覆盖用户处理授权弹窗的时间，避免过早放行导致后续业务请求失败
                request.timeout = Mathf.Max(1, options.IOSNetworkProbeTimeoutSeconds);
                request.SetRequestHeader("Cache-Control", "no-cache");

                try
                {
                    await request.SendWebRequest();
                    return !IsRequestError(request);
                }
                catch
                {
                    // 请求异常与请求失败同样视为网络不可用
                    return false;
                }
            }
        }

        [DllImport("__Internal")]
        private static extern int GetNetworkPermissionStatus();
#endif

        private static bool IsRequestError(UnityWebRequest request)
        {
#if UNITY_2020_2_OR_NEWER
            return request.result != UnityWebRequest.Result.Success;
#elif UNITY_2017_1_OR_NEWER
            return request.isNetworkError || request.isHttpError;
#else
            return request.isError;
#endif
        }
    }
}
