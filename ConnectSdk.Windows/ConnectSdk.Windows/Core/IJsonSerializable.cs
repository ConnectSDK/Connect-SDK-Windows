using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    public interface IJsonSerializable
    {
        JsonObject ToJsonObject();
    }
}