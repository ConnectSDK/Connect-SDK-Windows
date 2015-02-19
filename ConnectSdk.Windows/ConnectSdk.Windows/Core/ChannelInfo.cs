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
        public sbyte SourceIndex { get; set; }
        public sbyte PhysicalNumber { get; set; }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"name", JsonValue.CreateStringValue(ChannelName)},
                {"id", JsonValue.CreateStringValue(ChannelId)},
                {"number", JsonValue.CreateStringValue(ChannelNumber)},
                {"majorNumber", JsonValue.CreateNumberValue(MinorNumber)},
                {"minorNumber", JsonValue.CreateNumberValue(MinorNumber)},
                {"rawData", JsonValue.CreateStringValue(RawData.ToString())},
                {"sourceIndex", JsonValue.CreateStringValue(PhysicalNumber.ToString())},
                {"physicalNumber", JsonValue.CreateStringValue(SourceIndex.ToString())}};
            return obj;
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ChannelInfo)obj);
        }

        protected bool Equals(ChannelInfo other)
        {
            return string.Equals(ChannelName, other.ChannelName) && string.Equals(ChannelId, other.ChannelId) && string.Equals(ChannelNumber, other.ChannelNumber) && MinorNumber == other.MinorNumber && MajorNumber == other.MajorNumber && Equals(RawData, other.RawData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (ChannelName != null ? ChannelName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ChannelId != null ? ChannelId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ChannelNumber != null ? ChannelNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MinorNumber;
                hashCode = (hashCode * 397) ^ MajorNumber;
                hashCode = (hashCode * 397) ^ PhysicalNumber;
                hashCode = (hashCode * 397) ^ SourceIndex;
                hashCode = (hashCode * 397) ^ (RawData != null ? RawData.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}