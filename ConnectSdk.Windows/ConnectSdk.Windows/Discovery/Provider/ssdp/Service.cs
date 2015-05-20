#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Service.cs
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
using System.Collections.Generic;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Service
    {
        public static string Tag = "service";
        public static string TagServiceType = "serviceType";
        public static string TagServiceId = "serviceId";
        public static string TagScpdUrl = "SCPDURL";
        public static string TagControlUrl = "controlURL";
        public static string TagEventsubUrl = "eventSubURL";

        public string BaseUrl;

        /// <summary>
        /// Required. UPnP service type. 
        /// </summary>
        public string ServiceType;

        /// <summary>
        /// Required. Service identifier.  
        /// </summary>
        public string ServiceId;

        /// <summary>
        /// Required. Relative URL for service description. 
        /// </summary>
        public string ScpdUrl;

        /// <summary>
        /// Required. Relative URL for control. 
        /// </summary>
        public string ControlUrl;

        /// <summary>
        /// Relative. Relative URL for eventing. 
        /// </summary>
        public string EventSubUrl;

        public List<Action> ActionList { get; set; }
        public List<StateVariable> ServiceStateTable { get; set; }
    }
}