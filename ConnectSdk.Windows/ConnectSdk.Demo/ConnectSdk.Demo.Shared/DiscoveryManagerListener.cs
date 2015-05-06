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
                App.ApplicationModel.DiscoverredDeviceServices.Add(new DeviceServiceViewModel() { Device = device, Service = deviceService });
            }
        }

        public void OnDeviceUpdated(DiscoveryManager manager, ConnectableDevice device)
        {
            foreach (var deviceService in device.GetServices())
            {
                var f = (from t in App.ApplicationModel.DiscoverredDeviceServices
                    where t.Service.ServiceConfig.ServiceUuid == deviceService.ServiceConfig.ServiceUuid
                    select t).FirstOrDefault();
                if (f == null)
                    App.ApplicationModel.DiscoverredDeviceServices.Add(new DeviceServiceViewModel() { Device = device, Service = deviceService });
            }
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