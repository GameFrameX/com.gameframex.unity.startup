using System.Collections.Generic;

namespace GameFrameX.Startup.Runtime
{
    public interface IStartupHttpParams
    {
        string ToJson();

        Dictionary<string, object> ToDictionary();
    }
}
