using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    public class ProgramList : IJsonSerializable
    {
        public ChannelInfo Channel { get; private set; }
        public JsonArray ProgramsList { get; private set; }

        public ProgramList(ChannelInfo channel, JsonArray programList)
        {
            Channel = channel;
            ProgramsList = programList;
        }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"channel", JsonValue.CreateStringValue(Channel != null ? Channel.ToString() : "")},
                {"programList", JsonValue.CreateStringValue(ProgramsList != null ? ProgramsList.ToString() : "")}
            };
            return obj;
        }
    }
}