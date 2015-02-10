using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service;
using MyRemote.ConnectSDK.Device;
using MyRemote.ConnectSDK.Discovery;
using MyRemote.ConnectSDK.Service;
using MyRemote.ConnectSDK.Service.Command;
using MyRemote.ConnectSDK.Service.Config;

namespace ConnectSdk.Windows.Device
{
    /// <summary>
    /// ConnectableDevice serves as a normalization layer between your app and each of the device's services. It consolidates a lot of key data about the physical device and provides access to underlying functionality.
    /// ConnectableDevice consolidates some key information about the physical device, including model name, friendly name, ip address, connected DeviceService names, etc. In some cases, it is not possible to accurately select which DeviceService has the best friendly name, model name, etc. In these cases, the values of these properties are dependent upon the order of DeviceService discovery.
    /// To be informed of any ready/pairing/disconnect messages from each of the DeviceService, you must set a listener.
    /// ConnectableDevice exposes capabilities that exist in the underlying DeviceServices such as TV Control, Media Player, Media Control, Volume Control, etc. These capabilities, when accessed through the ConnectableDevice, will be automatically chosen from the most suitable DeviceService by using that DeviceService's CapabilityPriorityLevel.
    /// </summary>
    public class ConnectableDevice : IDeviceServiceListener
    {
        // ReSharper disable InconsistentNaming
        public static string KEY_ID = "id";
        public static string KEY_LAST_IP = "lastKnownIPAddress";
        public static string KEY_FRIENDLY = "friendlyName";
        public static string KEY_MODEL_NAME = "modelName";
        public static string KEY_MODEL_NUMBER = "modelNumber";
        public static string KEY_LAST_SEEN = "lastSeenOnWifi";
        public static string KEY_LAST_CONNECTED = "lastConnected";
        public static string KEY_LAST_DETECTED = "lastDetection";
        public static string KEY_SERVICES = "services";
        // ReSharper restore InconsistentNaming

        private string id;
        private ServiceDescription serviceDescription;
        private readonly List<IConnectableDeviceListener> listeners = new List<IConnectableDeviceListener>();
        private readonly Dictionary<string, DeviceService> services = new Dictionary<string, DeviceService>();

        public bool FeaturesReady = false;

        /// <summary> 
        /// Universally unique id of this particular ConnectableDevice object, persists between sessions in ConnectableDeviceStore for connected devices
        /// </summary>
        public string Id
        {
            get { return id ?? (id = Guid.NewGuid().ToString()); }
            set { id = value; }
        }

        public string DeviceType
        {
            get
            {
                try
                {
                    return (from t in services select t.Key).FirstOrDefault();
                }
                catch
                {
                    return "Unknown type of device";
                }
            }
        }

        public string IpAddress { get; set; }

        public string FriendlyName { get; set; }

        public string ModelName { get; set; }

        public string ModelNumber { get; set; }

        public string LastKnownIpAddress { get; set; }

        public string LastSeenOnWifi { get; set; }

        public double LastConnected { get; set; }

        public double LastDetection { get; set; }

        public ConnectableDevice(string ipAddress, string friendlyName, string modelName, string modelNumber)
        {
            IpAddress = ipAddress;
            FriendlyName = friendlyName;
            ModelName = modelName;
            ModelNumber = modelNumber;
        }

        public ConnectableDevice(ServiceDescription description)
        {
            Update(description);
        }

        public ConnectableDevice(JsonObject json)
        {
            Id = json.GetNamedString(KEY_ID);
            LastKnownIpAddress = json.GetNamedString(KEY_LAST_IP);
            FriendlyName = json.GetNamedString(KEY_FRIENDLY);
            ModelName = json.GetNamedString(KEY_MODEL_NAME);
            ModelNumber = json.GetNamedString(KEY_MODEL_NUMBER);
            LastSeenOnWifi = json.GetNamedString(KEY_LAST_SEEN);
            LastConnected = json.GetNamedNumber(KEY_LAST_CONNECTED);
            LastDetection = json.GetNamedNumber(KEY_LAST_DETECTED);

            var jsonServices = json.GetNamedObject(KEY_SERVICES);
            if (jsonServices != null)
            {
                foreach (var key in jsonServices.Keys)
                {
                    var jsonService = jsonServices.GetNamedObject(key);

                    if (jsonService != null)
                    {
                        var newService = DeviceService.getService(jsonService);
                        if (newService != null)
                            AddService(newService);
                    }
                }
            }
        }

        public static ConnectableDevice CreateFromConfigstring(string ipAddress, string friendlyName, string modelName,
            string modelNumber)
        {
            return new ConnectableDevice(ipAddress, friendlyName, modelName, modelNumber);
        }

        public static ConnectableDevice CreateWithId(string id, string ipAddress, string friendlyName, string modelName,
            string modelNumber)
        {
            var mDevice = new ConnectableDevice(ipAddress, friendlyName, modelName, modelNumber) { Id = id };
            return mDevice;
        }

        public ServiceDescription GetServiceDescription()
        {
            return serviceDescription;
        }

        public virtual void SetServiceDescription(ServiceDescription sourceServiceDescription)
        {
            serviceDescription = sourceServiceDescription;
        }

        /// <summary>
        /// Adds a DeviceService to the ConnectableDevice instance. Only one instance of each DeviceService type (webOS, Netcast, etc) may be attached to a single ConnectableDevice instance. If a device contains your service type already, your service will not be added.
        /// </summary>
        /// <param name="service">DeviceService to be added</param>
        public void AddService(DeviceService service)
        {
            var added = getMismatchCapabilities(service.Capabilities, GetCapabilities());

            service.setListener(this);

            foreach (var listener in listeners)
                listener.OnCapabilityUpdated(this, added, new List<string>());

            services.Add(service.ServiceName, service);
        }

        /// <summary>
        /// Removes a DeviceService from the ConnectableDevice instance.
        /// </summary>
        /// <param name="service">DeviceService to be removed</param>
        public void RemoveService(DeviceService service)
        {
            RemoveServiceWithId(service.ServiceName);
        }

        /// <summary>
        /// Removes a DeviceService from the ConnectableDevice instance.
        /// </summary>
        /// <param name="serviceId">serviceId ID of the DeviceService to be removed (DLNA, webOS TV, etc)</param>
        public void RemoveServiceWithId(string serviceId)
        {
            var service = services[serviceId];

            if (service == null)
                return;

            service.disconnect();

            services.Remove(serviceId);

            var removed = getMismatchCapabilities(service.Capabilities, GetCapabilities());

            foreach (var listener in listeners)
                listener.OnCapabilityUpdated(this, new List<string>(), removed);
        }

        private List<string> getMismatchCapabilities(IEnumerable<string> capabilities, ICollection<string> allCapabilities)
        {
            return capabilities.Where(cap => !allCapabilities.Contains(cap)).ToList();
        }

        /// <summary> 
        /// Array of all currently discovered DeviceServices this ConnectableDevice has associated with it. 
        /// </summary>
        public List<DeviceService> GetServices()
        {
            return services.Values.ToList();
        }

        /// <summary>
        /// Obtains a service from the ConnectableDevice with the provided serviceName
        /// </summary>
        /// <param name="serviceName">serviceName Service ID of the targeted DeviceService (webOS, Netcast, DLNA, etc)</param>
        /// <returns>DeviceService with the specified serviceName or nil, if none exists</returns>
        public DeviceService GetServiceByName(string serviceName)
        {
            return GetServices().FirstOrDefault(service => service.ServiceName.Equals(serviceName));
        }

        /// <summary>
        /// Removes a DeviceService form the ConnectableDevice instance.  serviceName is used as the identifier because 
        /// only one instance of each DeviceService type may be attached to a single ConnectableDevice instance.
        /// </summary>
        /// <param name="serviceName">Name of the DeviceService to be removed from the ConnectableDevice.</param>
        public void RemoveServiceByName(string serviceName)
        {
            RemoveService(GetServiceByName(serviceName));
        }

        /// <summary>
        /// Returns a DeviceService from the ConnectableDevice instance. serviceUUID is used as the identifier because only one 
        /// instance of each DeviceService type may be attached to a single ConnectableDevice instance.
        /// </summary>
        /// <param name="serviceUuid">UUID of the DeviceService to be returned</param>
        public DeviceService GetServiceWithUuid(string serviceUuid)
        {
            return GetServices().FirstOrDefault(service => service.ServiceDescription.Uuid.Equals(serviceUuid));
        }

        ///// <summary>
        ///// Adds the ConnectableDeviceListener to the list of listeners for this ConnectableDevice to receive certain events.
        ///// </summary>
        ///// <param name="listener">ConnectableDeviceListener to listen to device events (connect, disconnect, ready, etc)</param>
        //public void AddListener(IConnectableDeviceListener listener)
        //{
        //    if (listeners.Contains(listener) == false)
        //    {
        //        listeners.Add(listener);
        //    }
        //}

        ///// <summary>
        ///// Clears the array of listeners and adds the provided `listener` to the array. If `listener` is null, the array will be empty.
        ///// </summary>
        ///// <param name="listener">ConnectableDeviceListener to listen to device events (connect, disconnect, ready, etc)</param>
        //public void SetListener(IConnectableDeviceListener listener)
        //{
        //    listeners = new List<IConnectableDeviceListener>();

        //    if (listener != null)
        //        listeners.Add(listener);
        //}

        ///// <summary>
        ///// Removes a previously added ConenctableDeviceListener from the list of listeners for this ConnectableDevice.
        ///// 
        ///// @param listener ConnectableDeviceListener to be removed
        ///// </summary>
        //public void RemoveListener(IConnectableDeviceListener listener)
        //{
        //    listeners.Remove(listener);
        //}

        //public List<IConnectableDeviceListener> GetListeners()
        //{
        //    return listeners;
        //}

        /// <summary>
        /// Enumerates through all DeviceServices and attempts to connect to each of them. When all of a ConnectableDevice's DeviceServices are ready to receive commands, the ConnectableDevice will send a onDeviceReady message to its listener.
        ///
        /// It is always necessary to call connect on a ConnectableDevice, even if it contains no connectable DeviceServices.
        /// </summary>
        public void Connect()
        {
            foreach (var service in services.Values.Where(service => !service.isConnected()))
            {
                service.connect();
            }
        }

        /// <summary>
        /// Enumerates through all DeviceServices and attempts to disconnect from each of them.
        /// </summary>
        public void Disconnect()
        {
            foreach (var service in services.Values)
            {
                service.disconnect();
            }

            foreach (var listener in listeners)
                listener.OnDeviceDisconnected(this);
        }

        /// <summary>
        /// Checks if all services are connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return services.Values.All(service => service.isConnected());
        }

        /// <summary>
        /// Whether the device has any DeviceServices that require an active connection (websocket, HTTP registration, etc)
        /// </summary>
        public bool IsConnectable()
        {
            return services.Values.Any(service => service.isConnectable());
        }

        /// <summary> 
        /// A combined list of all capabilities that are supported among the detected DeviceServices. 
        /// </summary>
        public List<string> GetCapabilities()
        {
            var caps = new List<string>();
            foreach (var capability in from service in services.Values from capability in service.Capabilities where !caps.Contains(capability) select capability)
            {
                caps.Add(capability);
            }
            return caps;
        }

        /// <summary>
        /// Test to see if the capabilities array contains a given capability. See the individual Capability classes for acceptable capability values.
        /// It is possible to append a wildcard search term `.Any` to the end of the search term. This method will return true for capabilities that match the term up to the wildcard.
        /// </summary>
        /// <param name="capability">Capability to test against</param>
        /// <example>
        /// Launcher.App.Any
        /// </example>
        public bool HasCapability(string capability)
        {
            return services.Values.Any(service => service.hasCapability(capability));
        }

        /// <summary>
        /// Test to see if the capabilities array contains at least one capability in a given set of capabilities. See the individual Capability classes for acceptable capability values.
        /// See hasCapability: for a description of the wildcard feature provided by this method.
        /// </summary>
        public bool HasAnyCapability(List<string> capabilities)
        {
            return services.Values.Any(service => service.hasAnyCapability(capabilities));
        }

        /// <summary>
        /// Test to see if the capabilities array contains a given set of capabilities. See the individual Capability classes for acceptable capability values.
        /// See hasCapability: for a description of the wildcard feature provided by this method.
        /// </summary>
        /// <param name="capabilities">Capabilities Array of capabilities to test against</param>
        public bool HasCapabilities(List<string> capabilities)
        {
            return capabilities.All(HasCapability);
        }

        /// <summary>
        /// Gets a device as an implementation of one of these types: ILauncher, MediaPlayer, MediaControl, VolumeControl, 
        /// WebAppLauncher, TvControl, ToastControl, TextInputControl MouseControl, ExternalInputControl, PowerControl, KeyControl
        /// </summary>
        /// <typeparam name="T">The type of the control we want to retrieve</typeparam>
        /// <returns>The current device as the given type</returns>
        public T GetControl<T>() where T : class
        {
            T[] foundKeyControl = {default(T)};
            foreach (var keyControl in services.Values.OfType<T>().Select(service => service).Where(keyControl => foundKeyControl[0] == null))
            {
                foundKeyControl[0] = keyControl;
            }
            return foundKeyControl[0];
        }

        public string GetConnectedServiceNames()
        {
            var serviceCount = services.Count;

            if (serviceCount <= 0)
                return null;

            var serviceNames = new string[serviceCount];
            var serviceIndex = 0;

            foreach (var service in services)
            {
                serviceNames[serviceIndex] = service.Value.ServiceName;

                serviceIndex++;
            }

            // credit: http://stackoverflow.com/a/6623121/2715
            var sb = new StringBuilder();

            foreach (var serviceName in serviceNames)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(serviceName);
            }

            return sb.ToString();
        }

        public void Update(ServiceDescription description)
        {
            IpAddress = description.IpAddress;
            FriendlyName = description.FriendlyName;
            ModelName = description.ModelName;
            ModelNumber = description.ModelNumber;
            LastConnected = description.LastDetection;
        }

        public JsonObject ToJsonObject()
        {
            var deviceObject = new JsonObject
            {
                {KEY_ID, JsonValue.CreateStringValue(Id)},
                {KEY_LAST_IP, JsonValue.CreateStringValue(IpAddress)},
                {KEY_LAST_SEEN, JsonValue.CreateStringValue(LastSeenOnWifi ?? Util.GetTime().ToString())},
                {KEY_LAST_CONNECTED, JsonValue.CreateNumberValue(LastConnected)},
                {KEY_LAST_DETECTED, JsonValue.CreateNumberValue(LastDetection)}
            //deviceObject.Add(KEY_FRIENDLY, JsonValue.CreateStringValue(FriendlyName));
            //deviceObject.Add(KEY_MODEL_NAME, JsonValue.CreateStringValue(ModelName));
            //deviceObject.Add(KEY_MODEL_NUMBER, JsonValue.CreateStringValue(ModelNumber));
            };


            var jsonServices = new JsonObject();
            foreach (var service in services.Values)
            {
                var serviceObject = service.toJSONObject();

                jsonServices.Add(service.ServiceConfig.ServiceUuid, serviceObject);
            }
            deviceObject.Add(KEY_SERVICES, jsonServices);

            return deviceObject;
        }

        public void OnCapabilitiesUpdated(DeviceService service, List<string> added, List<string> removed)
        {
            DiscoveryManager.GetInstance().OnCapabilityUpdated(this, added, removed);
        }

        public void OnConnectionFailure(DeviceService service, Exception error)
        {
        }

        public void OnConnectionRequired(DeviceService service)
        {
        }

        public void OnConnectionSuccess(DeviceService service)
        {
            //  TODO: iOS is passing to a function for when each service is ready on a device.  This is not implemented on Android.

            if (!IsConnected()) return;
            var deviceStore = DiscoveryManager.GetInstance().GetConnectableDeviceStore();
            if (deviceStore != null)
            {
                deviceStore.AddDevice(this);
            }

            foreach (var listener in listeners)
                listener.OnDeviceReady(this);

            LastConnected = Util.GetTime();
        }

        public void OnDisconnect(DeviceService service, Exception error)
        {
            foreach (var listener in listeners)
                listener.OnDeviceDisconnected(this);
        }

        public void OnPairingFailed(DeviceService service, Exception error)
        {
            foreach (var listener in listeners)
                listener.OnConnectionFailed(this,
                    new ServiceCommandError(0, "Failed to pair with service " + service.ServiceName, null));
        }

        public void OnPairingRequired(DeviceService service, PairingType pairingType, Object pairingData)
        {
            foreach (var listener in listeners)
                listener.OnPairingRequired(this, service, pairingType);
        }

        public void OnPairingSuccess(DeviceService service)
        {
        }
    }
}
