namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// Final result of URL failover execution.
    /// </summary>
    public sealed class UrlFailoverResult
    {
        public bool Success { get; }

        public string FailedUrl { get; }

        public string ErrorMessage { get; }

        public UrlFailoverResult(bool success, string failedUrl, string errorMessage)
        {
            Success = success;
            FailedUrl = failedUrl ?? string.Empty;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public static UrlFailoverResult Succeed()
        {
            return new UrlFailoverResult(true, string.Empty, string.Empty);
        }

        public static UrlFailoverResult Fail(string failedUrl, string errorMessage)
        {
            return new UrlFailoverResult(false, failedUrl, errorMessage);
        }
    }
}
