#region Copyright Notice
/*
 * ConnectSdk.Windows
 * IWebOstvServiceSocketClientListener.cs
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
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.WebOs
{
    public interface IWebOstvServiceSocketClientListener
    {
        void OnConnect();
        void OnCloseWithError(ServiceCommandError error);
        void OnFailWithError(ServiceCommandError error);

        void OnBeforeRegister(PairingType pairingType);
        void OnRegistrationFailed(ServiceCommandError error);
        bool OnReceiveMessage(JsonObject message);
    }
}