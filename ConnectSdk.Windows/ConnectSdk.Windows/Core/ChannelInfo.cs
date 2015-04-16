#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ChannelInfo.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 20-2-2015,
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 #endregion
using System;
using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about a TVs channels. This object is required to set the channel on a TV.
    /// </summary>
    public class ChannelInfo
    {
        private string channelNumber;
        public string ChannelName { get; set; }
        public string ChannelId { get; set; }

        public string ChannelNumber
        {
            get { return channelNumber; }
            set
            {
                channelNumber = value;
            }
        }

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