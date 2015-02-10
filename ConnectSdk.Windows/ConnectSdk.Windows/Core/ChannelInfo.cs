using System;
using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about a TVs channels. This object is required to set the channel on a TV.
    /// </summary>
    public class ChannelInfo
    {
        public string ChannelName { get; set; }
        public string ChannelId { get; set; }
        public string ChannelNumber { get; set; }
        public int MinorNumber { get; set; }
        public int MajorNumber { get; set; }
        public JsonObject RawData { get; set; }

        public override bool Equals(Object obj)
        {
            var info = obj as ChannelInfo;
            if (info != null)
            {
                var other = info;

                if (ChannelId != null)
                {
                    if (ChannelId.Equals(other.ChannelId))
                        return true;
                }
                else if (ChannelName != null && this.ChannelNumber != null)
                {
                    return ChannelName.Equals(other.ChannelName)
                           && ChannelNumber.Equals(other.ChannelNumber)
                           && MajorNumber == other.MajorNumber
                           && MinorNumber == other.MinorNumber;
                }
                return false;
            }

            return base.Equals(obj);
        }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"name", JsonValue.CreateStringValue(ChannelName)},
                {"id", JsonValue.CreateStringValue(ChannelId)},
                {"number", JsonValue.CreateStringValue(ChannelNumber)},
                {"majorNumber", JsonValue.CreateNumberValue(MinorNumber)},
                {"minorNumber", JsonValue.CreateNumberValue(MinorNumber)},
                {"rawData", JsonValue.CreateStringValue(RawData.ToString())}
            };
            return obj;
        }
    }
}