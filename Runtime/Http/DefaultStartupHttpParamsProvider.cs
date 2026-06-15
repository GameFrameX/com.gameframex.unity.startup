namespace GameFrameX.Startup.Runtime
{
    public class DefaultStartupHttpParamsProvider : IStartupHttpParamsProvider
    {
        public virtual IStartupHttpParams Create(StartupOptions options)
        {
            return StartupHttpParams.FromOptions(options);
        }
    }
}
