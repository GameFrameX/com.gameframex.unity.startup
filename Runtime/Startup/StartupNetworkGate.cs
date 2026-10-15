using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动网络门禁。用于吸收 iOS 首次联网授权期间的首包失败。
    /// </summary>
    internal static class StartupNetworkGate
    {
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
            while (true)
            {
                uiHandler?.SetTipText("Waiting for network permission...");

                using (var request = UnityWebRequest.Get(options.IOSNetworkProbeUrl))
                {
                    request.timeout = Mathf.Max(1, options.IOSNetworkProbeTimeoutSeconds);
                    request.SetRequestHeader("Cache-Control", "no-cache");

                    await request.SendWebRequest();

                    if (!IsRequestError(request))
                    {
                        return;
                    }
                }

                await UniTask.Delay(Mathf.Max(300, options.IOSNetworkProbeRetryDelayMs));
            }
        }
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