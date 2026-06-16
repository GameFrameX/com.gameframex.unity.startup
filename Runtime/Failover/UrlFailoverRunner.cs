using System;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// Runs ordered URL failover with bounded retry attempts per URL.
    /// </summary>
    public static class UrlFailoverRunner
    {
        public static UniTask<UrlFailoverResult> ExecuteAsync(IReadOnlyList<string> urls, int maxAttemptsPerUrl, int retryDelayMs, Func<string, UniTask<UrlAttemptResult>> attempt, Action<string, int, int> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (urls == null)
            {
                throw new ArgumentNullException(nameof(urls));
            }

            if (urls.Count == 0)
            {
                throw new ArgumentException("URL list must contain at least one URL.", nameof(urls));
            }

            if (maxAttemptsPerUrl < 1)
            {
                throw new ArgumentException("Max attempts per URL must be greater than zero.", nameof(maxAttemptsPerUrl));
            }

            if (retryDelayMs < 0)
            {
                throw new ArgumentException("Retry delay must be zero or greater.", nameof(retryDelayMs));
            }

            if (attempt == null)
            {
                throw new ArgumentNullException(nameof(attempt));
            }

            return ExecuteCoreAsync(urls, maxAttemptsPerUrl, retryDelayMs, attempt, onProgress, cancellationToken);
        }

        private static async UniTask<UrlFailoverResult> ExecuteCoreAsync(IReadOnlyList<string> urls, int maxAttemptsPerUrl, int retryDelayMs, Func<string, UniTask<UrlAttemptResult>> attempt, Action<string, int, int> onProgress,
            CancellationToken cancellationToken)
        {
            var lastFailedUrl = string.Empty;
            var lastErrorMessage = string.Empty;

            for (var urlIndex = 0; urlIndex < urls.Count; urlIndex++)
            {
                var url = urls[urlIndex] ?? string.Empty;

                for (var attemptIndex = 1; attemptIndex <= maxAttemptsPerUrl; attemptIndex++)
                {
                    onProgress?.Invoke(url, attemptIndex, maxAttemptsPerUrl);

                    var result = await attempt(url);
                    if (result.Success)
                    {
                        return UrlFailoverResult.Succeed();
                    }

                    lastFailedUrl = url;
                    lastErrorMessage = result.ErrorMessage;

                    if (attemptIndex < maxAttemptsPerUrl && retryDelayMs > 0)
                    {
                        await UniTask.Delay(retryDelayMs, cancellationToken: cancellationToken);
                    }
                }
            }

            return UrlFailoverResult.Fail(lastFailedUrl, lastErrorMessage);
        }
    }
}
