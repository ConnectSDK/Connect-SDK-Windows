#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOstvServiceSocketClientListener.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 17-4-2015,
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
using Windows.Data.Json;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.WebOs
{
    public class WebOstvServiceSocketClientListener : IWebOstvServiceSocketClientListener
    {
        private readonly IDeviceServiceListener listener;
        private readonly DeviceService service;

        public WebOstvServiceSocketClientListener(DeviceService deviceService, IDeviceServiceListener deviceListener)
        {
            listener = deviceListener;
            service = deviceService;
        }

        public void OnConnect()
        {
            listener.OnConnectionSuccess(service);
        }

        public void OnCloseWithError(ServiceCommandError error)
        {
            service.Disconnect();
            if (listener != null && error != null)
                listener.OnConnectionFailure(service, new Exception(error.GetCode().ToString()));
        }

        public void OnFailWithError(ServiceCommandError error)
        {
            if (listener != null)
                listener.OnConnectionFailure(service, new Exception(error.GetCode().ToString()));
        }

        public void OnBeforeRegister(PairingType pairingType)
        {
            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                if (listener != null)
                    listener.OnPairingRequired(service, pairingType, null);
            }
        }

        public void OnRegistrationFailed(ServiceCommandError error)
        {
            service.Disconnect();
            if (listener != null)
                listener.OnConnectionFailure(service, new Exception(error.GetCode().ToString()));
        }

        public bool OnReceiveMessage(JsonObject message)
        {
            return true;
        }
    }
}