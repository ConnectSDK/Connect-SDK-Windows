using System;
using System.Collections.Generic;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Command;
using UpdateControls.Collections;

namespace ConnectSdk.Demo.Demo
{

    public class DiscoveryManagerListener : IDiscoveryManagerListener, IConnectableDeviceListener
    {
        private Dictionary<string, string> keys = null;

        public EventHandler<object> Paired;

        public void OnDeviceAdded(DiscoveryManager manager, ConnectableDevice device)
        {
            App.ApplicationModel.DiscoverredTvList.Add(device);
        }

        public void OnDeviceUpdated(DiscoveryManager manager, ConnectableDevice device)
        {
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
            throw new NotImplementedException();
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