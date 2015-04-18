#region Copyright Notice
/*
 * ConnectSdk.Windows
 * CapabilityFilter.cs
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
using System.Collections.Generic;

namespace ConnectSdk.Windows.Discovery
{
    public class CapabilityFilter
    {
        private List<string> capabilities = new List<string>();

        /// <summary>
        /// Create a CapabilityFilter with the given array of required capabilities.
        /// </summary>
        /// <param name="capabilities">capabilities List of capability names (see capability class files for string constants)</param>
        public CapabilityFilter(List<string> capabilities)
        {
            AddCapabilities(capabilities);
        }

        /// <summary>
        /// List of capabilities required by this filter. This property is readonly -- use the addCapability or addCapabilities to build this object.
        /// </summary>
        public List<string> Capabilities
        {
            get { return capabilities; }
            set { capabilities = value; }
        }

        /// <summary>
        /// Add a required capability to the filter.
        /// </summary>
        /// <param name="capability">capability Capability name to add (see capability class files for string constants)</param>
        public void AddCapability(string capability)
        {
            Capabilities.Add(capability);
        }


        /// <summary>
        /// Add array of required capabilities to the filter. (see capability class files for string constants)
        /// </summary>
        /// <param name="capabilitiesParam">capabilities List of capability names </param>
        public void AddCapabilities(List<string> capabilitiesParam)
        {
            Capabilities.AddRange(capabilitiesParam);
        }
    }
}
