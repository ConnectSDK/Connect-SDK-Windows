using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Demo.Demo
{

    public class DiscoveryManagerListener : IDiscoveryManagerListener, IConnectableDeviceListener
    {
        public EventHandler<object> Paired;

        public void OnDeviceAdded(DiscoveryManager manager, ConnectableDevice device)
        {
            foreach (var deviceService in device.GetServices())
            {
                App.ApplicationModel.DiscoverredTvList.Add(new DeviceServiceViewModel() {Device = device, Service = deviceService});
            }
            //if (App.ApplicationModel.DiscoverredTvList.FirstOrDefault(x => x.DeviceType == device.DeviceType && x.IpAddress == device.IpAddress) == null)
            //    App.ApplicationModel.DiscoverredTvList.Add(device);
        }

        public void OnDeviceUpdated(DiscoveryManager manager, ConnectableDevice device)
        {
            foreach (var deviceService in device.GetServices())
            {
                var f = (from t in App.ApplicationModel.DiscoverredTvList
                    where t.Service.ServiceConfig.ServiceUuid == deviceService.ServiceConfig.ServiceUuid
                    select t).FirstOrDefault();
                if (f == null)
                    App.ApplicationModel.DiscoverredTvList.Add(new DeviceServiceViewModel() { Device = device, Service = deviceService });
            }
            //throw new NotImplementedException();
        }

        public void OnDeviceRemoved(DiscoveryManager manager, ConnectableDevice device)
        {
            throw new NotImplementedException();
        }

        public void OnDiscoveryFailed(DiscoveryManager manager, ServiceCommandError error)
        {
            throw new NotImplementedException();
        }

        public void OnDeviceReady(ConnectableDevice device)
        {
            if (Paired != null)
                Paired(this, device);
        }

        public void OnDeviceDisconnected(ConnectableDevice device)
        {
            throw new NotImplementedException();
        }

        public void OnPairingRequired(ConnectableDevice device, DeviceService service, PairingType pairingType)
        {
            
        }

        public void OnCapabilityUpdated(ConnectableDevice device, List<string> added, List<string> removed)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionFailed(ConnectableDevice device, ServiceCommandError error)
        {
            throw new NotImplementedException();
        }
    }
}