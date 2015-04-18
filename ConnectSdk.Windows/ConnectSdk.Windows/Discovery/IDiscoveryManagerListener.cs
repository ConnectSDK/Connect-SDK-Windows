#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IDiscoveryManagerListener.cs
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
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Discovery
{
    /// <summary>
    /// The DiscoveryManagerListener will receive events on the addition/removal/update of ConnectableDevice objects.
    /// It is important to note that, unless you are implementing your own device picker, this listener is not needed in your code. Connect SDK's DevicePicker internally acts a separate listener to the DiscoveryManager and handles all of the same method calls.
    /// </summary>
    public interface IDiscoveryManagerListener
    {
        ///  <summary>
        /// This method will be fired upon the first discovery of one of a ConnectableDevice's DeviceServices.
        /// </summary>
        /// <param name="manager">DiscoveryManager that found the device</param>
        /// <param name="device">ConnectableDevice that was found</param>
        void OnDeviceAdded(DiscoveryManager manager, ConnectableDevice device);

        /// <summary>
        /// This method is called when a ConnectableDevice gains or loses a DeviceService in discovery.
        /// </summary>
        /// <param name="manager">DiscoveryManager that updated device</param>
        /// <param name="device">ConnectableDevice that was updated</param>
        void OnDeviceUpdated(DiscoveryManager manager, ConnectableDevice device);

        /// <summary>
        /// This method is called when connections to all of a ConnectableDevice's DeviceServices are lost. This will usually happen when a device is powered off or loses internet connectivity.
        /// </summary>
        /// <param name="manager">DiscoveryManager that lost device</param>
        /// <param name="device">ConnectableDevice that was lost</param>
        void OnDeviceRemoved(DiscoveryManager manager, ConnectableDevice device);

        /// <summary>
        /// In the event of an error in the discovery phase, this method will be called.
        /// </summary>
        /// <param name="manager">DiscoveryManager that experienced the error</param>
        /// <param name="error">NSError with a description of the failure</param>
        void OnDiscoveryFailed(DiscoveryManager manager, ServiceCommandError error);
    }
}
