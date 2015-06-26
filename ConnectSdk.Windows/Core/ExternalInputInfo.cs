#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ExternalInputInfo.cs
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
using Windows.Data.Json;

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about a DeviceService's external inputs. This object is required to set a DeviceService's external input.
    /// </summary>
    public class ExternalInputInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Connected { get; set; }
        public string IconUrl { get; set; }
        public JsonObject RawData { get; set; }

        public JsonObject ToJsonObject()
        {
            var obj = new JsonObject
            {
                {"id", JsonValue.CreateStringValue(Id)},
                {"name", JsonValue.CreateStringValue(Name)},
                {"connected", JsonValue.CreateBooleanValue(Connected)},
                {"icon", JsonValue.CreateStringValue(IconUrl)},
                {"rawData", JsonValue.CreateStringValue(RawData.ToString())}
            };
            return obj;
        }

        public static ExternalInputInfo FromJson(string json)
        {
            var jobj = JsonObject.Parse(json);
            var ei = new ExternalInputInfo
            {
                Id = jobj.GetNamedString("id", ""),
                Name = jobj.GetNamedString("name", ""),
                Connected = jobj.GetNamedBoolean("connected", false),
                IconUrl = jobj.GetNamedString("icon", ""),
                RawData = jobj
            };

            return ei;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ExternalInputInfo) obj);
        }

        protected bool Equals(ExternalInputInfo other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Name, other.Name) && Connected.Equals(other.Connected) && string.Equals(IconUrl, other.IconUrl) && Equals(RawData, other.RawData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Connected.GetHashCode();
                hashCode = (hashCode * 397) ^ (IconUrl != null ? IconUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RawData != null ? RawData.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}