#region Copyright Notice
/*
 * ConnectSdk.Windows
 * NetcastTVServiceConfig.cs
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
using Windows.Data.Json;

namespace ConnectSdk.Windows.Service.Config
{
    public class NetcastTvServiceConfig : ServiceConfig
    {
        public static string KeyPairing = "pairingKey";
        private string pairingKey;

        public string PairingKey
        {
            get { return pairingKey; }
            set
            {
                pairingKey = value;
                NotifyUpdate();
            }
        }

        public NetcastTvServiceConfig(string serviceUuid) :
            base(serviceUuid)
        {
        }

        public NetcastTvServiceConfig(string serviceUuid, string pairingKey)
            : base(serviceUuid)
        {
            PairingKey = pairingKey;
        }

        public NetcastTvServiceConfig(JsonObject json)
            : base(json)
        {
            PairingKey = json.GetNamedString(KeyPairing);
        }

        public override JsonObject ToJsonObject()
        {
            var jsonObj = base.ToJsonObject();

            try
            {
                jsonObj.Add(KeyPairing, JsonValue.CreateStringValue(PairingKey));
            }
            catch (Exception e)
            {
                throw e;
            }

            return jsonObj;
        }

    }
}