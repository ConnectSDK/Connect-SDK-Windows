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
        // ReSharper disable InconsistentNaming
        public static string TAG = "service";
        public static string TAG_SERVICE_TYPE = "serviceType";
        public static string TAG_SERVICE_ID = "serviceId";
        public static string TAG_SCPD_URL = "SCPDURL";
        public static string TAG_CONTROL_URL = "controlURL";
        public static string TAG_EVENTSUB_URL = "eventSubURL";
        // ReSharper restore InconsistentNaming

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