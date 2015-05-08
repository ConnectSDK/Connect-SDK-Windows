#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DiscoveryManager.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Discovery
{
    public class DiscoveryManager : IConnectableDeviceListener, IDiscoveryProviderListener, IServiceConfigListener
    {
        public enum PairingLevelEnum
        {
            Off,
            On
        }
        public static String ConnectSdkVersion = "1.4";

        private static DiscoveryManager instance;

        private IConnectableDeviceStore connectableDeviceStore;

        private readonly ConcurrentDictionary<string, ConnectableDevice> allDevices;
        private readonly ConcurrentDictionary<string, ConnectableDevice> compatibleDevices;

        private readonly Dictionary<string, Type> deviceClasses;
        private readonly List<IDiscoveryProvider> discoveryProviders;

        private readonly List<IDiscoveryManagerListener> discoveryListeners;
        private List<CapabilityFilter> capabilityFilters;

        private bool mSearching;

        public PairingLevelEnum PairingLevel { get; set; }

        public static void Init()
        {
            instance = new DiscoveryManager();
        }

        public static void Destroy()
        {
            instance.OnDestroy();
        }

        public static void Init(IConnectableDeviceStore connectableDeviceStore)
        {
            instance = new DiscoveryManager(connectableDeviceStore);
        }

        public static DiscoveryManager GetInstance()
        {
            if (instance == null)
                throw new Exception("Call DiscoveryManager.Init(Context) first");

            return instance;
        }

        public DiscoveryManager()
            : this(new DefaultConnectableDeviceStore(""))
        {

        }

        public DiscoveryManager(IConnectableDeviceStore connectableDeviceStore)
        {
            this.connectableDeviceStore = connectableDeviceStore;

            allDevices = new ConcurrentDictionary<string, ConnectableDevice>();
            compatibleDevices = new ConcurrentDictionary<string, ConnectableDevice>();

            deviceClasses = new Dictionary<string, Type>();
            discoveryProviders = new List<IDiscoveryProvider>();

            discoveryListeners = new List<IDiscoveryManagerListener>();

            capabilityFilters = new List<CapabilityFilter>();
            PairingLevel = PairingLevelEnum.Off;
        }

        public void AddListener(IDiscoveryManagerListener listener)
        {
            // notify listener of all devices so far
            foreach (var device in compatibleDevices.Values)
            {
                listener.OnDeviceAdded(this, device);
            }
            discoveryListeners.Add(listener);
        }

        public void RemoveListener(IDiscoveryManagerListener listener)
        {
            discoveryListeners.Remove(listener);
        }


        public void SetCapabilityFilters(List<CapabilityFilter> capabilityFiltersParam)
        {
            capabilityFilters = capabilityFiltersParam;

            foreach (var device in compatibleDevices.Values)
            {
                HandleDeviceLoss(device);
            }

            compatibleDevices.Clear();

            foreach (var device in allDevices.Values.Where(DeviceIsCompatible))
            {
                compatibleDevices.TryAdd(device.IpAddress, device);

                HandleDeviceAdd(device);
            }
        }

        public List<CapabilityFilter> GetCapabilityFilters()
        {
            return capabilityFilters;
        }

        public bool DeviceIsCompatible(ConnectableDevice device)
        {
            if (capabilityFilters == null || capabilityFilters.Count == 0)
            {
                return true;
            }

            return capabilityFilters.Any(filter => device.HasCapabilities(filter.Capabilities));
        }

        public void RegisterDefaultDeviceTypes()
        {
            Dictionary<Type, Type> devicesList = DefaultPlatform.GetDeviceServiceMap();
            foreach (var pair in  devicesList)
            {
                RegisterDeviceService(pair.Key, pair.Value);
            }
        }


        private static bool IsAssignableFrom(Type from, Type to)
        {
            return (from.GetTypeInfo().BaseType == to || from.GetTypeInfo().ImplementedInterfaces.Contains(to));
        }

        public void RegisterDeviceService(Type deviceClass, Type discoveryClass)
        {
            if (!IsAssignableFrom(deviceClass, typeof (DeviceService)))
                return;
            if (!IsAssignableFrom(discoveryClass, typeof (IDiscoveryProvider)))
                return;

            var discoveryProvider = discoveryProviders.FirstOrDefault(dp => dp.GetType() == discoveryClass);

            if (discoveryProvider == null)
            {
                discoveryProvider = Activator.CreateInstance(discoveryClass) as IDiscoveryProvider;

                if (discoveryProvider != null)
                {
                    discoveryProvider.AddListener(this);
                    discoveryProviders.Add(discoveryProvider);
                }
            }
            var m = deviceClass.GetRuntimeMethod("DiscoveryFilter", new Type[] { });
            var discoveryFilter = m.Invoke(discoveryProvider, new object[] { }) as DiscoveryFilter;

            if (discoveryFilter != null)
            {
                var serviceId = discoveryFilter.ServiceId;
                deviceClasses.Add(serviceId, deviceClass);
                if (discoveryProvider != null) discoveryProvider.AddDeviceFilter(discoveryFilter);
            }
            
        }

        public void UnregisterDeviceService(Type deviceClass, Type discoveryClass)
        {
            if (deviceClass != typeof (DeviceService))
                return;
            if (discoveryClass != typeof (IDiscoveryProvider))
                return;

            IDiscoveryProvider discoveryProvider =
                discoveryProviders.FirstOrDefault(dp => dp.GetType() == discoveryClass);

            if (discoveryProvider == null)
                return;

            var m = deviceClass.GetRuntimeMethod("DiscoveryFilter", new Type[] { });
            var discoveryFilter = m.Invoke(discoveryProvider, new object[] { }) as DiscoveryFilter;

            discoveryProvider.RemoveDeviceFilter(discoveryFilter);

            if (!discoveryProvider.IsEmpty()) return;
            discoveryProvider.Stop();
            discoveryProviders.Remove(discoveryProvider);
        }

        public void Start()
        {
            if (mSearching)
                return;

            mSearching = true;

            if (discoveryProviders == null)
            {
                return;
            }

            if (discoveryProviders.Count == 0)
            {
                RegisterDefaultDeviceTypes();
            }

            if (Util.IsWirelessAvailable())
            {
                foreach (var provider in discoveryProviders)
                {
                    provider.Start();
                }
            }
            else
            {
                Logger.Current.AddMessage("Wifi is not connected yet");
                foreach (var discoveryManagerListener in discoveryListeners)
                {
                    discoveryManagerListener.OnDiscoveryFailed(this,
                        new ServiceCommandError(0, null));
                }
            }
        }

        public void Stop()
        {
            if (!mSearching)
                return;

            mSearching = false;

            foreach (var provider in discoveryProviders)
            {
                provider.Stop();
            }
        }

        public void SetConnectableDeviceStore(IConnectableDeviceStore connectableDeviceStoreParam)
        {
            connectableDeviceStore = connectableDeviceStoreParam;
        }

        public IConnectableDeviceStore GetConnectableDeviceStore()
        {
            return connectableDeviceStore;
        }

        public void HandleDeviceAdd(ConnectableDevice device)
        {
            if (!DeviceIsCompatible(device))
                return;

            if (!compatibleDevices.ContainsKey(device.IpAddress))
                compatibleDevices.TryAdd(device.IpAddress, device);

            foreach (var listenter in discoveryListeners)
            {
                listenter.OnDeviceAdded(this, device);
            }
        }

        public void HandleDeviceUpdate(ConnectableDevice device)
        {
            if (DeviceIsCompatible(device))
            {
                if (device.IpAddress != null && compatibleDevices.ContainsKey(device.IpAddress))
                {
                    foreach (var listenter in discoveryListeners)
                    {
                        listenter.OnDeviceUpdated(this, device);
                    }
                }
                else
                {
                    HandleDeviceAdd(device);
                }
            }
            else
            {
                ConnectableDevice c;
                compatibleDevices.TryRemove(device.IpAddress,out c);
                HandleDeviceLoss(device);
            }
        }

        public void HandleDeviceLoss(ConnectableDevice device)
        {
            foreach (var listenter in discoveryListeners)
            {
                listenter.OnDeviceRemoved(this, device);
            }

            device.Disconnect();
        }

        public bool IsNetcast(ServiceDescription description)
        {
            bool isNetcastTv = false;

            var modelName = description.ModelName;
            var modelDescription = description.ModelDescription;

            if (modelName != null && modelName.ToUpper().Equals("LG TV"))
            {
                if (modelDescription != null && !(modelDescription.ToUpper().Contains("WEBOS")))
                {
                    if (description.ServiceId.Equals(NetcastTvService.Id)) 
                    {
                        isNetcastTv = true;
                    }
                }
            }

            return isNetcastTv;
        }

        public ConcurrentDictionary<string, ConnectableDevice> GetAllDevices()
        {
            return allDevices;
        }

        public ConcurrentDictionary<string, ConnectableDevice> GetCompatibleDevices()
        {
            return compatibleDevices;
        }

        public void OnDestroy()
        {

        }

        public void OnServiceConfigUpdate(ServiceConfig serviceConfig)
        {
            if (connectableDeviceStore == null)
            {
                return;
            }
            foreach (ConnectableDevice device in allDevices.Values)
            {
                if (null != device.GetServiceWithUuid(serviceConfig.ServiceUuid))
                {
                    connectableDeviceStore.UpdateDevice(device);
                }
            }
        }

        public void OnCapabilityUpdated(ConnectableDevice device, List<string> added, List<string> removed)
        {
            HandleDeviceUpdate(device);
        }

        public void OnConnectionFailed(ConnectableDevice device, ServiceCommandError error)
        {
        }

        public void OnDeviceDisconnected(ConnectableDevice device)
        {
        }

        public void OnDeviceReady(ConnectableDevice device)
        {
        }

        public void OnPairingRequired(ConnectableDevice device, DeviceService service, PairingType pairingType)
        {
        }

        public void OnServiceAdded(IDiscoveryProvider provider, ServiceDescription serviceDescription)
        {
            Logger.Current.AddMessage(string.Format("Service added: {0}({1})", serviceDescription.FriendlyName, serviceDescription.ServiceId));
            var deviceIsNew = !allDevices.ContainsKey(serviceDescription.IpAddress);

            var device =
                (from d in allDevices where d.Key == serviceDescription.IpAddress select d.Value).FirstOrDefault();

            if (device == null)
            {
                device = new ConnectableDevice(serviceDescription) {IpAddress = serviceDescription.IpAddress};
                allDevices.TryAdd(serviceDescription.IpAddress, device);
                deviceIsNew = true;
            }

            device.LastDetection = Util.GetTime();
            device.LastKnownIpAddress = serviceDescription.IpAddress;
            //  TODO: Implement the currentSSID Property in DiscoveryManager
            //		device.setLastSeenOnWifi(currentSSID);

            AddServiceDescriptionToDevice(serviceDescription, device);

            if (device.Services.Count == 0)
                return; // we get here when a non-LG DLNA TV is found

            if (deviceIsNew)
                HandleDeviceAdd(device);
            else
                HandleDeviceUpdate(device);
        }

        public void OnServiceRemoved(IDiscoveryProvider provider, ServiceDescription serviceDescription)
        {
            Logger.Current.AddMessage(serviceDescription == null
                ? string.Format("Service removed: {0}", "unknown service")
                : string.Format("Service removed: {0}", serviceDescription.FriendlyName));

            if (serviceDescription != null)
            {
                var device = allDevices[serviceDescription.IpAddress];

                if (device != null)
                {
                    device.RemoveServiceWithId(serviceDescription.ServiceId);

                    if (device.Services.Count == 0)
                    {
                        ConnectableDevice dev;
                        allDevices.TryRemove(serviceDescription.IpAddress,out dev);

                        HandleDeviceLoss(device);
                    }
                    else
                    {
                        HandleDeviceUpdate(device);
                    }
                }
            }
        }

        public void OnServiceDiscoveryFailed(IDiscoveryProvider provider, ServiceCommandError error)
        {
            Logger.Current.AddMessage(string.Format("Service discovery failed. {0}", error.GetPayload()));
        }

        public void AddServiceDescriptionToDevice(ServiceDescription desc, ConnectableDevice device)
        {
            Logger.Current.AddMessage(string.Format("Adding service: {0} to device with address {1} and id {2}", desc.ServiceId, device.IpAddress, device.Id));
            var deviceServiceClass = deviceClasses[desc.ServiceId];

            if (deviceServiceClass == null)
                return;

            if (deviceServiceClass == typeof (DlnaService))
            {
                if (desc.LocationXml == null)
                    return;
            }
            else if (deviceServiceClass == typeof (NetcastTvService))
            {
                if (!IsNetcast(desc))
                    return;
            }

            ServiceConfig serviceConfig = null;

            if (connectableDeviceStore != null)
                serviceConfig = connectableDeviceStore.GetServiceConfig(desc.Uuid);

            if (serviceConfig == null)
                serviceConfig = new ServiceConfig(desc);

            serviceConfig.Listener = this;

            var hasType = false;
            var hasService = false;

            foreach (DeviceService service in device.Services)
            {
                if (!service.ServiceDescription.ServiceId.Equals(desc.ServiceId)) continue;
                hasType = true;
                if (service.ServiceDescription.Uuid.Equals(desc.Uuid))
                {
                    hasService = true;
                }
                break;
            }

            if (hasType)
            {
                if (hasService)
                {
                    device.ServiceDescription = desc;

                    var alreadyAddedService = device.GetServiceByName(desc.ServiceId);

                    if (alreadyAddedService != null)
                        alreadyAddedService.SetServiceDescription(desc);

                    return;
                }

                device.RemoveServiceByName(desc.ServiceId);
            }

            var deviceService = DeviceService.GetService(deviceServiceClass, desc, serviceConfig);

            if (deviceService == null) return;

            deviceService.SetServiceDescription(desc);
            device.AddService(deviceService);


            //    Type deviceServiceClass;

            //    if (IsNetcast(desc))
            //    {
            //        deviceServiceClass = typeof(NetcastTvService);
            //        var m = deviceServiceClass.GetRuntimeMethod("discoveryParameters", new Type[] { });
            //        var result = m.Invoke(null, new object[0]);

            //        if (result == null)
            //            return;

            //        var discoveryParameters = (JsonObject)result;
            //        var serviceId = discoveryParameters.GetNamedString("serviceId");

            //        if (serviceId.Length == 0)
            //            return;

            //        desc.ServiceId = serviceId;
            //    }
            //    else
            //    {
            //        deviceServiceClass = deviceClasses[desc.ServiceId];
            //    }

            //    if (deviceServiceClass == null)
            //        return;

            //    if (typeof(DlnaService) == deviceServiceClass)
            //    {
            //        const string netcast = "netcast";
            //        const string webos = "webos";

            //        var locNet = desc.LocationXml.IndexOf(netcast, StringComparison.Ordinal);
            //        var locWeb = desc.LocationXml.IndexOf(webos, StringComparison.Ordinal);

            //        if (locNet == -1 && locWeb == -1)
            //            return;
            //    }

            //    var serviceConfig = new ServiceConfig(desc) {Listener = this};

            //    var hasType = false;
            //    var hasService = false;

            //    foreach (var service in device.GetServices().Where(service => service.ServiceDescription.ServiceId.Equals(desc.ServiceId)))
            //    {
            //        hasType = true;
            //        if (service.ServiceDescription.Uuid.Equals(desc.Uuid))
            //        {
            //            hasService = true;
            //        }
            //        break;
            //    }

            //    if (hasType)
            //    {
            //        if (hasService)
            //        {
            //            device.SetServiceDescription(desc);

            //            var alreadyAddedService = device.GetServiceByName(desc.ServiceId);

            //            if (alreadyAddedService != null)
            //                alreadyAddedService.ServiceDescription = desc;

            //            return;
            //        }

            //        device.RemoveServiceByName(desc.ServiceId);
            //    }

            //    var deviceService = DeviceService.GetService(deviceServiceClass, desc, serviceConfig);
            //    deviceService.ServiceDescription = desc;
            //    device.AddService(deviceService);
            //}
        }
    }
}