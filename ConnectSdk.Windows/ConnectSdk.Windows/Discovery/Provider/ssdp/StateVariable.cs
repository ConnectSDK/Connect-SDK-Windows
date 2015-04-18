#region Copyright Notice
/*
 * ConnectSdk.Windows
 * StateVariable.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 16-4-2015,
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
namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class StateVariable
    {
        // ReSharper disable InconsistentNaming
        public static string TAG = "stateVariable";
        public static string TAG_NAME = "name";
        public static string TAG_DATA_TYPE = "dataType";
        // ReSharper restore InconsistentNaming

        public StateVariable()
        {
            Multicast = "no";
            SendEvents = "yes";
        }

        /// <summary>
        /// Optional. Defines whether event messages will be generated when the value
        /// of this state variable changes. Defaut value is "yes".
        /// </summary>
        public string SendEvents { get; set; }

        /// <summary>
        /// Optional. Defines whether event messages will be delivered using
        /// multicast eventing. Default value is "no".
        /// </summary>
        public string Multicast { get; set; }

        /// <summary>
        /// Required. Name of state variable.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Required. Same as data types defined by XML Schema.
        /// </summary>
        public string DataType { get; set; }
    }
}