namespace GameFrameX.Startup.Runtime
{
    public interface IStartupHttpParamsProvider
    {
        IStartupHttpParams Create(StartupOptions options);
    }
}
