#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DefaultPlatform.cs
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
using System;
using System.Collections.Generic;
using ConnectSdk.Windows.Discovery.Provider;
using ConnectSdk.Windows.Service;

namespace ConnectSdk.Windows.Device
{
    public class DefaultPlatform
    {
        public static Dictionary<Type, Type> GetDeviceServiceMap()
        {
            var devicesList = new Dictionary<Type, Type>
            {
                {typeof (WebOstvService), typeof (SsdpDiscoveryProvider)},
                {typeof (NetcastTvService), typeof (SsdpDiscoveryProvider)}
            };
            //devicesList.Add(typeof(DLNAService), typeof(SsdpDiscoveryProvider));
            //devicesList.Add(typeof(DIALService), typeof(SsdpDiscoveryProvider));
            //devicesList.Add(typeof(RokuService), typeof(SsdpDiscoveryProvider));
            //devicesList.Add(typeof(CastService), typeof(CastDiscoveryProvider));
            //devicesList.Add(typeof(AirPlayService), typeof(ZeroconfDiscoveryProvider));
            return devicesList;
        }

    }
}