using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Windows.Management.Deployment;
using Windows.UI.Core;
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


            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (App.ApplicationModel.DiscoverredDevices.All(x => x.Id != device.Id))
                {
                    App.ApplicationModel.AddDevice(device);
                }
             });
        }

        public void OnDeviceUpdated(DiscoveryManager manager, ConnectableDevice device)
        {
            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (App.ApplicationModel.DiscoverredDevices.Contains(device))
                {
                    App.ApplicationModel.DiscoverredDevices.Remove(device);
                    App.ApplicationModel.DiscoverredDevices.Add(device);
                    device.OnPropertyChanged("ServiceNames");
                }
            });
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
            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (App.ApplicationModel.Connected)
                    App.ApplicationModel.OnDeviceDisconnected(EventArgs.Empty);
            });
        }

        public void OnPairingRequired(ConnectableDevice device, DeviceService service, PairingType pairingType)
        {

        }

        public void OnCapabilityUpdated(ConnectableDevice device, List<string> added, List<string> removed)
        {
            //throw new NotImplementedException();
        }

        public void OnConnectionFailed(ConnectableDevice device, ServiceCommandError error)
        {
            throw new NotImplementedException();
        }
    }
}