#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ConnectableDevice.cs
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
using System.Text;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;

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
        public static string KeyId = "id";
        public static string KeyLastIp = "lastKnownIPAddress";
        public static string KeyFriendly = "friendlyName";
        public static string KeyModelName = "modelName";
        public static string KeyModelNumber = "modelNumber";
        public static string KeyLastSeen = "lastSeenOnWifi";
        public static string KeyLastConnected = "lastConnected";
        public static string KeyLastDetected = "lastDetection";
        public static string KeyServices = "services";

        private string id;
        private List<IConnectableDeviceListener> listeners = new List<IConnectableDeviceListener>();
        private readonly ConcurrentDictionary<string, DeviceService> services = new ConcurrentDictionary<string, DeviceService>();

        public bool FeaturesReady = false;

        /// <summary> 
        /// Universally unique id of this particular ConnectableDevice object, persists between sessions in ConnectableDeviceStore for connected devices
        /// </summary>
        public string Id
        {
            get { return id ?? (id = Guid.NewGuid().ToString()); }
            set { id = value; }
        }


        public string IpAddress { get; set; }
        public string FriendlyName { get; set; }
        public string ModelName { get; set; }
        public string ModelNumber { get; set; }
        public string LastKnownIpAddress { get; set; }
        public string LastSeenOnWifi { get; set; }
        public double LastConnected { get; set; }
        public double LastDetection { get; set; }
        public ServiceDescription ServiceDescription { get; set; }

        public ConnectableDevice()
        {
            services = new ConcurrentDictionary<String, DeviceService>();
        }

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
            services = new ConcurrentDictionary<String, DeviceService>();

            Id = json.GetNamedString(KeyId);
            LastKnownIpAddress = json.GetNamedString(KeyLastIp);
            FriendlyName = json.GetNamedString(KeyFriendly);
            ModelName = json.GetNamedString(KeyModelName);
            ModelNumber = json.GetNamedString(KeyModelNumber);
            LastSeenOnWifi = json.GetNamedString(KeyLastSeen);
            LastConnected = json.GetNamedNumber(KeyLastConnected);
            LastDetection = json.GetNamedNumber(KeyLastDetected);
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

        /// <summary>
        /// Adds a DeviceService to the ConnectableDevice instance. Only one instance of each DeviceService type (webOS, Netcast, etc) may be attached to a single ConnectableDevice instance. If a device contains your service type already, your service will not be added.
        /// </summary>
        /// <param name="service">DeviceService to be added</param>
        public void AddService(DeviceService service)
        {
            var added = GetMismatchCapabilities(service.Capabilities, GetCapabilities());

            service.Listener = this;

            foreach (var listener in listeners)
                listener.OnCapabilityUpdated(this, added, new List<string>());

            services.TryAdd(service.ServiceName, service);
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

            service.Disconnect();
            DeviceService srv;
            services.TryRemove(serviceId, out srv);

            var removed = GetMismatchCapabilities(service.Capabilities, GetCapabilities());

            foreach (var listener in listeners)
                listener.OnCapabilityUpdated(this, new List<string>(), removed);
        }

        private static List<string> GetMismatchCapabilities(IEnumerable<string> capabilities, ICollection<string> allCapabilities)
        {
            return capabilities.Where(cap => !allCapabilities.Contains(cap)).ToList();
        }

        /// <summary> 
        /// Array of all currently discovered DeviceServices this ConnectableDevice has associated with it. 
        /// </summary>
        public List<DeviceService> Services
        {
            get
            {
                return services.Values.ToList();
            }
        }

        /// <summary>
        /// Obtains a service from the ConnectableDevice with the provided serviceName
        /// </summary>
        /// <param name="serviceName">serviceName Service ID of the targeted DeviceService (webOS, Netcast, DLNA, etc)</param>
        /// <returns>DeviceService with the specified serviceName or nil, if none exists</returns>
        public DeviceService GetServiceByName(string serviceName)
        {
            return Services.FirstOrDefault(service => service.ServiceName.Equals(serviceName));
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
            return Services.FirstOrDefault(service => service.ServiceDescription.Uuid.Equals(serviceUuid));
        }

        /// <summary>
        /// Adds the ConnectableDeviceListener to the list of listeners for this ConnectableDevice to receive certain events.
        /// </summary>
        /// <param name="listener">ConnectableDeviceListener to listen to device events (connect, disconnect, ready, etc)</param>
        public void AddListener(IConnectableDeviceListener listener)
        {
            if (listeners.Contains(listener) == false)
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Clears the array of listeners and adds the provided `listener` to the array. If `listener` is null, the array will be empty.
        /// </summary>
        /// <param name="listener">ConnectableDeviceListener to listen to device events (connect, disconnect, ready, etc)</param>
        public void SetListener(IConnectableDeviceListener listener)
        {
            listeners = new List<IConnectableDeviceListener>();

            if (listener != null)
                listeners.Add(listener);
        }

        /// <summary>
        /// Removes a previously added ConenctableDeviceListener from the list of listeners for this ConnectableDevice.
        /// 
        /// @param listener ConnectableDeviceListener to be removed
        /// </summary>
        public void RemoveListener(IConnectableDeviceListener listener)
        {
            listeners.Remove(listener);
        }

        public List<IConnectableDeviceListener> GetListeners()
        {
            return listeners;
        }

        /// <summary>
        /// Enumerates through all DeviceServices and attempts to connect to each of them. When all of a ConnectableDevice's DeviceServices are ready to receive commands, the ConnectableDevice will send a onDeviceReady message to its listener.
        ///
        /// It is always necessary to call connect on a ConnectableDevice, even if it contains no connectable DeviceServices.
        /// </summary>
        public void Connect()
        {
            foreach (var service in services.Values.Where(service => !service.IsConnected()))
            {
                service.Connect();
            }
        }

        /// <summary>
        /// Enumerates through all DeviceServices and attempts to disconnect from each of them.
        /// </summary>
        public void Disconnect()
        {
            foreach (var service in services.Values)
            {
                service.Disconnect();
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
            return services.Values.All(service => service.IsConnected() || service.IsConnectable());
        }

        /// <summary>
        /// Whether the device has any DeviceServices that require an active connection (websocket, HTTP registration, etc)
        /// </summary>
        public bool IsConnectable()
        {
            return services.Values.Any(service => service.IsConnectable());
        }


        /// <summary>
        /// Sends a pairing key to all discovered device services.
        /// </summary>
        /// <param name="pairingKey">Pairing key to send to services</param>
        public void SendPairingKey(String pairingKey)
        {
            foreach (var service in services.Values)
            {
                service.SendPairingKey(pairingKey);
            }
        }

        /// <summary>
        /// Explicitly cancels pairing on all services that require pairing. In some services, this will hide a prompt that is displaying on the device.
        /// </summary>
        public void CancelPairing()
        {
            foreach (var service in services.Values)
            {
                service.CancelPairing();
            }
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
            return services.Values.Any(service => service.HasCapability(capability));
        }

        /// <summary>
        /// Test to see if the capabilities array contains at least one capability in a given set of capabilities. See the individual Capability classes for acceptable capability values.
        /// See hasCapability: for a description of the wildcard feature provided by this method.
        /// </summary>
        public bool HasAnyCapability(List<string> capabilities)
        {
            return services.Values.Any(service => service.HasAnyCapability(capabilities));
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

        public T GetApiController<T>(string priorityMethod) where T : class, ICapabilityMethod
        {
            DeviceService foundController = null;
            foreach (var service in services.Values)
            {
                if (service.GetApi<T>() == null)
                    continue;

                var controller = service.GetApi<T>();

                if (foundController == null)
                {
                    foundController = controller as DeviceService;
                }
                else
                {
                    var method = typeof(T).GetRuntimeMethods().FirstOrDefault(x => x.Name.Equals(priorityMethod));
                    if (method != null)
                    {
                        var controllerProirity =
                            (CapabilityPriorityLevel) method.Invoke(controller, null);
                        var foundControllerProirity =
                            (CapabilityPriorityLevel) method.Invoke(foundController, null);
                        if (controllerProirity > foundControllerProirity)
                        {
                            foundController = controller as DeviceService;
                        }
                    }
                }
            }
            return foundController as T;
        }

        /// <summary>
        /// Gets the highest priority capability of a service
        /// </summary>
        /// <param name="clazz"></param>
        /// <returns></returns>
        public T GetCapability<T>() where T : class, ICapabilityMethod
        {
            var method = "";
            if (typeof(T) == typeof(ILauncher)) method = "GetLauncherCapabilityLevel";
            if (typeof(T) ==  typeof(IMediaPlayer)) method = "GetMediaPlayerCapabilityLevel";
            if (typeof(T) == typeof(IMediaControl)) method = "GetMediaControlCapabilityLevel";
            if (typeof(T) == typeof(IPlayListControl)) method = "GetPlaylistControlCapabilityLevel";
            if (typeof(T) == typeof(IVolumeControl)) method = "GetVolumeControlCapabilityLevel";
            if (typeof(T) == typeof(IWebAppLauncher)) method = "GetWebAppLauncherCapabilityLevel";
            if (typeof(T) == typeof(ITvControl)) method = "GetTvControlCapabilityLevel";
            if (typeof(T) == typeof(IToastControl)) method = "GetToastControlCapabilityLevel";
            if (typeof(T) == typeof(ITextInputControl)) method = "GetTextInputControlCapabilityLevel";
            if (typeof(T) == typeof(IMouseControl)) method = "GetMouseControlCapabilityLevel";

            if (typeof(T) == typeof(IExternalInputControl)) method = "GetExternalInputControlPriorityLevel";
            if (typeof(T) == typeof(IPowerControl)) method = "GetPowerControlCapabilityLevel";
            if (typeof(T) == typeof(IKeyControl)) method = "GetKeyControlCapabilityLevel";
            return GetApiController<T>(method);
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
                {KeyId, JsonValue.CreateStringValue(Id)},
                {KeyLastIp, JsonValue.CreateStringValue(IpAddress)},
                {KeyLastSeen, JsonValue.CreateStringValue(LastSeenOnWifi ?? Util.GetTime().ToString())},
                {KeyLastConnected, JsonValue.CreateNumberValue(LastConnected)},
                {KeyLastDetected, JsonValue.CreateNumberValue(LastDetection)},
                {KeyFriendly, JsonValue.CreateStringValue(FriendlyName)},
                {KeyModelName, JsonValue.CreateStringValue(ModelName)},
                {KeyModelNumber, JsonValue.CreateStringValue(ModelNumber)}
            };

            var jsonServices = new JsonObject();
            foreach (var service in services.Values)
            {
                var serviceObject = service.ToJsonObject();

                jsonServices.Add(service.ServiceConfig.ServiceUuid, serviceObject);
            }
            deviceObject.Add(KeyServices, jsonServices);

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
                    new ServiceCommandError(0, null));
        }

        public void OnPairingRequired(DeviceService service, PairingType pairingType, Object pairingData)
        {
            foreach (var listener in listeners)
                listener.OnPairingRequired(this, service, pairingType);
        }

        public void OnPairingSuccess(DeviceService service)
        {
        }

        //private int GetConnectedServiceCount()
        //{
        //    var count = 0;

        //    foreach (var service in services.Values)
        //    {
        //        if (service.IsConnectable())
        //        {
        //            if (service.IsConnected())
        //                count++;
        //        }
        //        else
        //        {
        //            count++;
        //        }
        //    }

        //    return count;
        //}
    }
}
