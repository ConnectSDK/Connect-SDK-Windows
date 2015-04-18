#region Copyright Notice
/*
 * ConnectSdk.Windows
 * URLServiceSubscription.cs
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
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class UrlServiceSubscription :  ServiceCommand, IServiceSubscription
    {
        private readonly List<ResponseListener> listeners = new List<ResponseListener>();

        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload,
            ResponseListener listener) :
                base(service, uri, payload, listener)
        {
        }


        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload, bool isWebOs,
            ResponseListener listener) :
                base(service, uri, payload, listener)
        {
            if (isWebOs)
                HttpMethod = "subscribe";
        }

        public new void Send()
        {
            Subscribe();
        }

        public void Subscribe()
        {
            if (!(HttpMethod.Equals(TypeGet)
                  || HttpMethod.Equals(TypePost)))
            {
                HttpMethod = "subscribe";
            }
            
            Service.SendCommand(this);

        }

        public void Unsubscribe()
        {
            //todo fix this
            Service.Unsubscribe(this);
        }

        public List<ResponseListener> GetListeners()
        {
            throw new System.NotImplementedException();
        }

        public ResponseListener AddListener(ResponseListener listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public void RemoveListener(ResponseListener listener)
        {
            listeners.Remove(listener);
        }

        public void RemoveListeners()
        {
            listeners.Clear();
        }


    }
}