using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Device;
using ConnectSdk.Windows.Discovery.Provider;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using MyRemote.ConnectSDK.Device;

namespace ConnectSdk.Windows.Discovery
{
    /**
     * ###Overview
     *
     * At the heart of Connect SDK is DiscoveryManager, a multi-protocol service discovery engine with a pluggable architecture. Much of your initial experience with Connect SDK will be with the DiscoveryManager class, as it consolidates discovered service information into ConnectableDevice objects.
     *
     * ###In depth
     * DiscoveryManager supports discovering services of differing protocols by using DiscoveryProviders. Many services are discoverable over [SSDP][0] and are registered to be discovered with the SSDPDiscoveryProvider class.
     *
     * As services are discovered on the network, the DiscoveryProviders will notify DiscoveryManager. DiscoveryManager is capable of attributing multiple services, if applicable, to a single ConnectableDevice instance. Thus, it is possible to have a mixed-mode ConnectableDevice object that is theoretically capable of more functionality than a single service can provide.
     *
     * DiscoveryManager keeps a running list of all discovered devices and maintains a filtered list of devices that have satisfied any of your CapabilityFilters. This filtered list is used by the DevicePicker when presenting the user with a list of devices.
     *
     * Only one instance of the DiscoveryManager should be in memory at a time. To assist with this, DiscoveryManager has static method at sharedManager.
     *
     * Example:
     *
     * @capability kMediaControlPlay
     *
     @code
        DiscoveryManager.init(getApplicationContext());
        DiscoveryManager discoveryManager = DiscoveryManager.getInstance();
        discoveryManager.addListener(this);
        discoveryManager.start();
     @endcode
     *
     * [0]: http://tools.ietf.org/html/draft-cai-ssdp-v1-03
     */

    public class DiscoveryManager : IConnectableDeviceListener, IDiscoveryProviderListener, IServiceConfigListener
    {

        public enum PairingLevel
        {
            // ReSharper disable InconsistentNaming
            OFF,
            ON
            // ReSharper restore InconsistentNaming
        }

        // @cond INTERNAL
        private static DiscoveryManager instance;

        //Context context;
        private IConnectableDeviceStore connectableDeviceStore;

        private readonly Dictionary<string, ConnectableDevice> allDevices;
        private readonly Dictionary<string, ConnectableDevice> compatibleDevices;

        private readonly Dictionary<string, Type> deviceClasses;
        private readonly List<IDiscoveryProvider> discoveryProviders;

        private readonly List<IDiscoveryManagerListener> discoveryListeners;
        private List<CapabilityFilter> capabilityFilters;

        //BroadcastReceiver receiver;
        private bool isBroadcastReceiverRegistered;

        private PairingLevel pairingLevel;

        private bool mSearching;
        private bool mShouldResume;

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
                throw new Exception("Call DiscoveryManager.init(Context) first");

            return instance;
        }

        public DiscoveryManager()
            : this(new DefaultConnectableDeviceStore(""))
        {

        }

        public DiscoveryManager(IConnectableDeviceStore connectableDeviceStore)
        {
            this.connectableDeviceStore = connectableDeviceStore;

            allDevices = new Dictionary<string, ConnectableDevice>();
            compatibleDevices = new Dictionary<string, ConnectableDevice>();

            deviceClasses = new Dictionary<string, Type>();
            discoveryProviders = new List<IDiscoveryProvider>();

            discoveryListeners = new List<IDiscoveryManagerListener>();

            capabilityFilters = new List<CapabilityFilter>();
            pairingLevel = PairingLevel.OFF;
        }

        private void RegisterBroadcastReceiver()
        {
            if (isBroadcastReceiverRegistered == false)
            {
                isBroadcastReceiverRegistered = true;
            }
        }

        private void UnregisterBroadcastReceiver()
        {
            if (isBroadcastReceiverRegistered)
            {
                isBroadcastReceiverRegistered = false;
            }
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
                compatibleDevices.Add(device.IpAddress, device);

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
            //RegisterDeviceService(typeof(WebOSTVService), typeof(SsdpDiscoveryProvider));
            RegisterDeviceService(typeof(NetcastTvService), typeof(SsdpDiscoveryProvider));
            //registerDeviceService(typeof(DIALService), typeof(SSDPDiscoveryProvider));
            //registerDeviceService(typeof(RokuService), typeof(SSDPDiscoveryProvider));
            //registerDeviceService(typeof(CastService), typeof(CastDiscoveryProvider));

            //registerDeviceService(typeof (DlnaService), typeof (SsdpDiscoveryProvider)); //  includes Netcast

            //registerDeviceService(typeof(AirPlayService), typeof(SSDPDiscoveryProvider));
        }


        private static bool IsAssignableFrom(Type from, Type to)
        {
            return (from.GetTypeInfo().BaseType == to || from.GetTypeInfo().ImplementedInterfaces.Contains(to));
        }

        public void RegisterDeviceService(Type deviceClass, Type discoveryClass)
        {
            if (!IsAssignableFrom(deviceClass, typeof(DeviceService)))
                return;
            if (!IsAssignableFrom(discoveryClass, typeof(IDiscoveryProvider)))
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
            var m = deviceClass.GetRuntimeMethod("DiscoveryParameters", new Type[] { });
            var result = m.Invoke(discoveryProvider, new object[] { }) as JsonObject;
            var discoveryParameters = result;
            if (discoveryParameters != null)
            {
                var serviceFilter = discoveryParameters.GetNamedString("serviceId");

                deviceClasses.Add(serviceFilter, deviceClass);
            }

            if (discoveryProvider != null) discoveryProvider.AddDeviceFilter(discoveryParameters);
        }

        public void UnregisterDeviceService(Type deviceClass, Type discoveryClass)
        {
            if (deviceClass != typeof(DeviceService))
                return;
            if (discoveryClass != typeof(IDiscoveryProvider))
                return;

            IDiscoveryProvider discoveryProvider = discoveryProviders.FirstOrDefault(dp => dp.GetType() == discoveryClass);

            if (discoveryProvider == null)
                return;

            var m = deviceClass.GetRuntimeMethod("discoveryParameters", new Type[] { });
            var result = m.Invoke(discoveryProvider, new object[] { }) as JsonObject;

            var discoveryParameters = result;
            if (discoveryParameters != null)
            {
                var serviceFilter = discoveryParameters.GetNamedString("serviceId");

                deviceClasses.Remove(serviceFilter);
            }

            discoveryProvider.RemoveDeviceFilter(discoveryParameters);

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

            if (mShouldResume)
            {
                mShouldResume = false;
            }
            else
            {
                RegisterBroadcastReceiver();
            }

            foreach (var provider in discoveryProviders)
            {
                provider.Start();
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
            if (!mShouldResume)
                UnregisterBroadcastReceiver();
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

            compatibleDevices.Add(device.IpAddress, device);

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
                compatibleDevices.Remove(device.IpAddress);
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
            var isNetcastTv = false;

            var modelName = description.ModelName;
            var modelDescription = description.ModelDescription;

            if (modelName == null || !modelName.ToUpper().Equals("LG TV")) return false;
            if (modelDescription != null && !(modelDescription.ToUpper().Contains("WEBOS")))
            {
                isNetcastTv = true;
            }

            return isNetcastTv;
        }

        public Dictionary<string, ConnectableDevice> GetAllDevices()
        {
            return allDevices;
        }

        public Dictionary<string, ConnectableDevice> GetCompatibleDevices()
        {
            return compatibleDevices;
        }

        public PairingLevel GetPairingLevel()
        {
            return pairingLevel;
        }

        public void SetPairingLevel(PairingLevel pairingLevelParam)
        {
            pairingLevel = pairingLevelParam;
        }

        public void OnDestroy()
        {

        }

        public void OnServiceConfigUpdate(ServiceConfig serviceConfig)
        {

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
            var deviceIsNew = !allDevices.ContainsKey(serviceDescription.IpAddress);

            var device = (from d in allDevices where d.Key == serviceDescription.IpAddress select d.Value).FirstOrDefault();
                
            if (device == null)
            {
                device = new ConnectableDevice(serviceDescription) { IpAddress = serviceDescription.IpAddress };
                allDevices.Add(serviceDescription.IpAddress, device);
                deviceIsNew = true;
            }

            device.LastDetection = Util.GetTime();
            device.LastKnownIpAddress = serviceDescription.IpAddress;
            //  TODO: Implement the currentSSID Property in DiscoveryManager
            //		device.setLastSeenOnWifi(currentSSID);

            AddServiceDescriptionToDevice(serviceDescription, device);

            if (device.GetServices().Count == 0)
                return; // we get here when a non-LG DLNA TV is found

            if (deviceIsNew)
                HandleDeviceAdd(device);
            else
                HandleDeviceUpdate(device);
        }

        public void OnServiceRemoved(IDiscoveryProvider provider, ServiceDescription serviceDescription)
        {
            var device = allDevices[serviceDescription.IpAddress];

            if (device != null)
            {
                device.RemoveServiceWithId(serviceDescription.ServiceId);

                if (device.GetServices().Count == 0)
                {
                    allDevices.Remove(serviceDescription.IpAddress);

                    HandleDeviceLoss(device);
                }
                else
                {
                    HandleDeviceUpdate(device);
                }
            }
        }

        public void OnServiceDiscoveryFailed(IDiscoveryProvider provider, ServiceCommandError error)
        {
        }

        public void AddServiceDescriptionToDevice(ServiceDescription desc, ConnectableDevice device)
        {
            Type deviceServiceClass;

            if (IsNetcast(desc))
            {
                deviceServiceClass = typeof(NetcastTvService);
                var m = deviceServiceClass.GetRuntimeMethod("discoveryParameters", new Type[] { });
                var result = m.Invoke(null, new object[0]);

                if (result == null)
                    return;

                var discoveryParameters = (JsonObject)result;
                var serviceId = discoveryParameters.GetNamedString("serviceId");

                if (serviceId.Length == 0)
                    return;

                desc.ServiceId = serviceId;
            }
            else
            {
                deviceServiceClass = deviceClasses[desc.ServiceId];
            }

            if (deviceServiceClass == null)
                return;

            if (typeof(DlnaService) == deviceServiceClass)
            {
                const string netcast = "netcast";
                const string webos = "webos";

                var locNet = desc.LocationXml.IndexOf(netcast, StringComparison.Ordinal);
                var locWeb = desc.LocationXml.IndexOf(webos, StringComparison.Ordinal);

                if (locNet == -1 && locWeb == -1)
                    return;
            }

            var serviceConfig = new ServiceConfig(desc) {Listener = this};

            var hasType = false;
            var hasService = false;

            foreach (var service in device.GetServices().Where(service => service.ServiceDescription.ServiceId.Equals(desc.ServiceId)))
            {
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
                    device.SetServiceDescription(desc);

                    var alreadyAddedService = device.GetServiceByName(desc.ServiceId);

                    if (alreadyAddedService != null)
                        alreadyAddedService.ServiceDescription = desc;

                    return;
                }

                device.RemoveServiceByName(desc.ServiceId);
            }

            var deviceService = DeviceService.GetService(deviceServiceClass, desc, serviceConfig);
            deviceService.ServiceDescription = desc;
            device.AddService(deviceService);
        }
    }
}
