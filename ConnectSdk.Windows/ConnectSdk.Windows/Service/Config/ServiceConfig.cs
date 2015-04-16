#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ServiceConfig.cs
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
using System;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;

namespace ConnectSdk.Windows.Service.Config
{
    public class ServiceConfig
    {
        public static string KeyClass = "class";
        public static string KeyLastDetect = "lastDetection";
        public static string KeyUuid = "UUID";
        private string serviceUuid;
        private double lastDetected = double.MaxValue;

        readonly bool connected;
        readonly bool wasConnected;

        private IServiceConfigListener listener;

        public string ServiceUuid
        {
            get { return serviceUuid; }
            set { serviceUuid = value; }
        }

        public double LastDetected
        {
            get { return lastDetected; }
            set { lastDetected = value; }
        }

        public IServiceConfigListener Listener
        {
            get { return listener; }
            set { listener = value; }
        }


        public ServiceConfig(string serviceUuid)
        {
            if (serviceUuid == null) throw new ArgumentNullException("serviceUuid");
            ServiceUuid = serviceUuid;
        }

        public ServiceConfig(ServiceDescription desc)
        {
            ServiceUuid = desc.Uuid;
            connected = false;
            wasConnected = false;
            LastDetected = Util.GetTime();
        }

        public ServiceConfig(ServiceConfig config)
        {
            ServiceUuid = config.ServiceUuid;
            connected = config.connected;
            wasConnected = config.wasConnected;
            LastDetected = config.LastDetected;

            listener = config.listener;
        }

        public ServiceConfig(JsonObject json)
        {
            ServiceUuid = json.GetNamedString(KeyUuid);
            LastDetected = json.GetNamedNumber(KeyLastDetect);
        }

        public static ServiceConfig GetConfig(JsonObject json)
        {
            ServiceConfig newServiceClass = null;
            var className = json.GetNamedString(KeyClass);
            if (className.Equals("ServiceConfig", StringComparison.OrdinalIgnoreCase))
            {
                newServiceClass =
                    (ServiceConfig)Activator.CreateInstance(typeof(ServiceConfig), new object[] { json });
            }
            if (className.Equals("NetcastTVServiceConfig", StringComparison.OrdinalIgnoreCase))
            {
                newServiceClass =
                    (NetcastTvServiceConfig)Activator.CreateInstance(typeof(NetcastTvServiceConfig), new object[] { json });
            }

            return newServiceClass;
        }


        public override string ToString()
        {
            return ServiceUuid;
        }

        public void Detect()
        {
            LastDetected = Util.GetTime();
        }

        public virtual JsonObject ToJsonObject()
        {
            var jsonObj = new JsonObject();

            try
            {
                jsonObj.Add(KeyClass, JsonValue.CreateStringValue(GetType().Name));
                jsonObj.Add(KeyLastDetect, JsonValue.CreateNumberValue(lastDetected));
                jsonObj.Add(KeyUuid, JsonValue.CreateStringValue(serviceUuid));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                //e.printStackTrace();
            }

            return jsonObj;
        }

        protected void NotifyUpdate()
        {
            if (listener != null)
            {
                listener.OnServiceConfigUpdate(this);
            }
        }
    }
}
