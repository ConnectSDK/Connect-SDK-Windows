using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    public interface IJsonDeserializable
    {
        void FromJsonObject(JsonObject obj);
    }
}