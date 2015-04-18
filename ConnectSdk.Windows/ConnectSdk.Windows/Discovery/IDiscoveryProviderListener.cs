#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IDiscoveryProviderListener.cs
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
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Discovery
{
    /// <summary>
    /// The DiscoveryProviderListener is mechanism for passing service information to the DiscoveryManager. You likely will not be using the DiscoveryProviderListener class directly, as DiscoveryManager acts as a listener to all of the DiscoveryProviders.
    /// </summary>
    public interface IDiscoveryProviderListener
    {
        /// <summary>
        /// This method is called when the DiscoveryProvider discovers a service that matches one of its DeviceService filters. The ServiceDescription is created and passed to the listener (which should be the DiscoveryManager). The ServiceDescription is used to create a DeviceService, which is then attached to a ConnectableDevice object. 
        /// </summary>
        /// <param name="provider">DiscoveryProvider that found the service</param>
        /// <param name="serviceDescription">ServiceDescription of the service that was found</param>
        void OnServiceAdded(IDiscoveryProvider provider, ServiceDescription serviceDescription);

        /// <summary>
        /// This method is called when the DiscoveryProvider's internal mechanism loses reference to a service that matches one of its DeviceService filters. 
        /// </summary>
        /// <param name="provider">DiscoveryProvider that lost the service</param>
        /// <param name="serviceDescription">ServiceDescription of the service that was lost</param>
        void OnServiceRemoved(IDiscoveryProvider provider, ServiceDescription serviceDescription);

        /// <summary>
        /// This method is called on any error/failure within the DiscoveryProvider.
        /// </summary>
        /// <param name="provider">DiscoveryProvider that failed</param>
        /// <param name="error">ServiceCommandError providing a information about the failure</param>
        void OnServiceDiscoveryFailed(IDiscoveryProvider provider, ServiceCommandError error);
    } 
}
