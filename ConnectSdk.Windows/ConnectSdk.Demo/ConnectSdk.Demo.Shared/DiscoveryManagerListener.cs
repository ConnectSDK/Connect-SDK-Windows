using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Demo.Demo
{

    public class DiscoveryManagerListener : IDiscoveryManagerListener, IConnectableDeviceListener
    {
        private Dictionary<string, string> keys = null;

        public void OnDeviceAdded(DiscoveryManager manager, ConnectableDevice device)
        {
            App.ApplicationModel.DiscoverredTvList = new ObservableCollection<ConnectableDevice> { device };
        }

        public void OnDeviceUpdated(DiscoveryManager manager, ConnectableDevice device)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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