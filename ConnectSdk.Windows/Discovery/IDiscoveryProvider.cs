#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IDiscoveryProvider.cs
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
namespace ConnectSdk.Windows.Discovery
{
    /// <summary>
    ///  ###Overview
    ///  From a high-level perspective, DiscoveryProvider completely abstracts the functionality of discovering services of a particular protocol (SSDP, Cast, etc). The DiscoveryProvider will pass service information to the DiscoveryManager, which will then create a DeviceService object and attach it to a ConnectableDevice object.
    /// 
    ///  ###In Depth
    ///  DiscoveryProvider is an abstract class that is meant to be extended. You shouldn't ever use DiscoveryProvider directly, unless extending it to provide support for another discovery protocol.
    /// 
    ///  By default, DiscoveryManager will set itself as a DiscoveryProvider's listener. You should not change the listener as it could cause unexpected inconsistencies within the discovery process.
    /// 
    ///  See CastDiscoveryProvider and SSDPDiscoveryProvider for implementations.
    /// </summary>
    public interface IDiscoveryProvider
    {
        /// <summary>
        ///  Starts the DiscoveryProvider.
        /// </summary>
        void Start();

        /// <summary>
        ///  Stops the DiscoveryProvider.
        /// </summary>
        void Stop();

        /// <summary>
        ///  Resets the DiscoveryProvider.
        /// </summary>
        void Reset();

        /// <summary>
        /// Restarts the provider
        /// </summary>
        void Restart();

        /// <summary>
        /// Sends out discovery query without a full restart
        /// </summary>
        void Rescan();

        /// <summary> 
        /// Adds a DiscoveryProviderListener, which should be the DiscoveryManager 
        /// </summary>
        void AddListener(IDiscoveryProviderListener listener);

        /// <summary> 
        /// Removes a DiscoveryProviderListener. 
        /// </summary>
        /// <param name="listener">The listener to be removed</param>
        void RemoveListener(IDiscoveryProviderListener listener);

        /// <summary>
        ///  Adds a device filter for a particular DeviceService.
        /// </summary>
        /// <param name="parameters">Parameters to be used for discovering a particular DeviceService</param>
        void AddDeviceFilter(DiscoveryFilter parameters);

        /// <summary>
        ///  Removes a device filter for a particular DeviceService. If the DiscoveryProvider has no other devices to be searching for, the DiscoveryProvider will be stopped and de-referenced.
        /// </summary>
        /// <param name="parameters">Parameters to be used for removing a particular DeviceService</param>
        void RemoveDeviceFilter(DiscoveryFilter parameters);

        /// <summary>
        ///  Whether or not the DiscoveryProvider has any services it is supposed to be searching for. If YES, then the DiscoveryProvider will be stopped and de-referenced by the DiscoveryManager.
        /// </summary>
        bool IsEmpty();
    }
}
