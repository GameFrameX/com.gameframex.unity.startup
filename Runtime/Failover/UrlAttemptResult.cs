namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// Result returned by a single URL attempt.
    /// </summary>
    public sealed class UrlAttemptResult
    {
        public bool Success { get; }

        public string ErrorMessage { get; }

        public UrlAttemptResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        public static UrlAttemptResult Succeed()
        {
            return new UrlAttemptResult(true, string.Empty);
        }

        public static UrlAttemptResult Fail(string errorMessage)
        {
            return new UrlAttemptResult(false, errorMessage);
        }
    }
}
