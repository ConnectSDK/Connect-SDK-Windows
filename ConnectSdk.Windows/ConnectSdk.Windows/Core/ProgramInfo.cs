#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ProgramInfo.cs
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

namespace ConnectSdk.Windows.Core
{
    /// <summary>
    /// Normalized reference object for information about a TVs program.
    /// </summary>
    public class ProgramInfo 
    {
        /// <summary>
        /// Gets or sets the ID of the program on the first screen device. Format is different depending on the platform.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly name of the program (ex. Sesame Street, Cosmos, Game of Thrones, etc).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the reference to the ChannelInfo object that this program is associated with
        /// </summary>
        public ChannelInfo ChannelInfo { get; set; }

        /// <summary>
        /// Gets or sets the raw data from the first screen device about the program. In most cases, this is an Dictionary.
        /// </summary>
        public object RawData { get; set; }

        /// <summary>
        /// Compares two ProgramInfo objects.
        /// </summary>
        /// <param name="other">ProgramInfo object to compare</param>
        /// <returns></returns>
        protected bool Equals(ProgramInfo other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Name, other.Name) && Equals(ChannelInfo, other.ChannelInfo) && Equals(RawData, other.RawData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ChannelInfo != null ? ChannelInfo.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (RawData != null ? RawData.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override bool Equals(Object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProgramInfo) obj);
        }
    }
}