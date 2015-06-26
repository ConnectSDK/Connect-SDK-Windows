#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ChannelInfo.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
        private string name;

        /// <summary>
        /// Gets or sets the user-friendly name of the channel
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the TV's unique ID for the channel
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the TV channel's number (likely to be a combination of the major & minor numbers)
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the TV channel's minor number
        /// </summary>
        public int MinorNumber { get; set; }

        /// <summary>
        /// Gets or sets the TV channel's major number
        /// </summary>
        public int MajorNumber { get; set; }

        /// <summary>
        /// Gets the raw data from the first screen device about the channel. In most cases, this is a Dictionary.
        /// </summary>
        public JsonObject RawData { get; set; }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"name", JsonValue.CreateStringValue(Name)},
                {"id", JsonValue.CreateStringValue(Id)},
                {"number", JsonValue.CreateStringValue(Number)},
                {"majorNumber", JsonValue.CreateNumberValue(MinorNumber)},
                {"minorNumber", JsonValue.CreateNumberValue(MinorNumber)},
                {"rawData", JsonValue.CreateStringValue(RawData.ToString())},
            };
            return obj;
        }

        /// <summary>
        /// Compares two ChannelInfo objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ChannelInfo)obj);
        }

        /// <summary>
        /// Compares two ChannelInfo objects.
        /// </summary>
        /// <param name="other">ChannelInfo object to compare.</param>
        /// <returns>YES if both ChannelInfo number & name values are equal</returns>
        protected bool Equals(ChannelInfo other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Id, other.Id) && string.Equals(Number, other.Number) && MinorNumber == other.MinorNumber && MajorNumber == other.MajorNumber && Equals(RawData, other.RawData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Number != null ? Number.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MinorNumber;
                hashCode = (hashCode * 397) ^ MajorNumber;
                hashCode = (hashCode * 397) ^ (RawData != null ? RawData.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}