#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IDeviceServiceListener.cs
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
using System.Collections.Generic;

namespace ConnectSdk.Windows.Service
{
    public interface IDeviceServiceListener
    {
        /// <summary>
        /// If the DeviceService requires an active connection (websocket, pairing, etc) this method will be called. 
        /// </summary>
        /// <param name="service">service DeviceService that requires connection</param>
        void OnConnectionRequired(DeviceService service);

        /// <summary>
        /// After the connection has been successfully established, and after pairing (if applicable), this method will be called.
        /// </summary>
        /// <param name="service">DeviceService that requires connection</param>
        void OnConnectionSuccess(DeviceService service);

        /// <summary>
        /// There are situations in which a DeviceService will update the capabilities it supports and propagate these changes to the DeviceService. Such situations include:
        /// on discovery, DIALService will reach out to detect if certain apps are installed
        /// on discovery, certain DeviceServices need to reach out for version & region information
        /// For more information on this particular method, see ConnectableDeviceDelegate's connectableDevice:capabilitiesAdded:removed: method.
        /// </summary>
        /// <param name="service">DeviceService that requires connection</param>
        /// <param name="added">List<string/> of capabilities that are new to the DeviceService</param>
        /// <param name="removed">List<string/> of capabilities that the DeviceService has lost</param>
        void OnCapabilitiesUpdated(DeviceService service, List<string> added, List<string> removed);

        /// <summary>
        /// This method will be called on any disconnection. If error is nil, then the connection was clean and likely triggered by the responsible DiscoveryProvider or by the user.
        /// </summary>
        /// <param name="service">DeviceService that requires connection</param>
        /// <param name="error">Error with a description of any errors causing the disconnect. If this value is nil, then the disconnect was clean/expected.</param>
        void OnDisconnect(DeviceService service, Exception error);

        /// <summary>
        /// Will be called if the DeviceService fails to establish a connection.
        /// </summary>
        /// <param name="service">DeviceService that requires connection</param>
        /// <param name="error">Error with a description of the failure</param>
        void OnConnectionFailure(DeviceService service, Exception error);

        /// <summary>
        /// If the DeviceService requires pairing, valuable data will be passed to the delegate via this method.
        /// </summary>
        /// <param name="service">DeviceService that requires connection</param>
        /// <param name="pairingType">PairingType that the DeviceService requires</param>
        /// <param name="pairingData">Any data that might be required for the pairing process, will usually be nil</param>
        void OnPairingRequired(DeviceService service, PairingType pairingType, Object pairingData);

        /// <summary>
        /// This method will be called upon pairing success. On pairing success, a connection to the DeviceService will be attempted.
        /// </summary>
        /// <param name="service">service DeviceService that has successfully completed pairing</param>
        void OnPairingSuccess(DeviceService service);


        /// <summary>
        /// If there is any error in pairing, this method will be called.
        /// </summary>
        /// <param name="service">service DeviceService that has failed to complete pairing</param>
        /// <param name="error">error Error with a description of the failure</param>
        void OnPairingFailed(DeviceService service, Exception error);
    }
}