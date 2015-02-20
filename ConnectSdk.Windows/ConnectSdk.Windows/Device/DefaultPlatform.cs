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