using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Config;
using MyRemote.ConnectSDK.Device;

namespace ConnectSdk.Windows.Device
{
    public class DefaultConnectableDeviceStore : IConnectableDeviceStore
    {
        // ReSharper disable InconsistentNaming
        private const int CURRENT_VERSION = 0;
        public static string KEY_VERSION = "version";

        public static string KEY_CREATED = "created";
        public static string KEY_UPDATED = "updated";
        public static string KEY_DEVICES = "devices";

        // ReSharper disable UnusedField.Compiler
        private static string DIRPATH = "/android/data/connect_sdk/";
        private static string FILENAME = "StoredDevices";

        private static string IP_ADDRESS = "ipAddress";
        private static string FRIENDLY_NAME = "friendlyName";
        private static string MODEL_NAME = "modelName";
        private static string MODEL_NUMBER = "modelNumber";
        private static string SERVICES = "services";
        private static string DESCRIPTION = "description";
        private static string CONFIG = "config";

        private static string FILTER = "filter";
        private static string UUID = "uuid";
        private static string PORT = "port";

        private static string SERVICE_UUID = "serviceUUID";
        private static string CLIENT_KEY = "clientKey";
        private static string SERVER_CERTIFICATE = "serverCertificate";
        private static string PAIRING_KEY = "pairingKey";

        private static string DEFAULT_SERVICE_WEBOSTV = "WebOSTVService";
        private static string DEFAULT_SERVICE_NETCASTTV = "NetcastTVService";
        // ReSharper restore UnusedField.Compiler
        // ReSharper restore InconsistentNaming

        /** Date (in seconds from 1970) that the ConnectableDeviceStore was created. */
        public long Created;
        /** Date (in seconds from 1970) that the ConnectableDeviceStore was last updated. */
        public long Updated;
        /** Current version of the ConnectableDeviceStore, may be necessary for migrations */
        public int Version;

        /// <summary>
        /// Max length of time for a ConnectableDevice to remain in the ConnectableDeviceStore without being discovered. Default is 3 days, and modifications to this value will trigger a scan for old devices.
        /// </summary>
        public long MaxStoreDuration = TimeSpan.TicksPerDay * 3 / TimeSpan.TicksPerSecond;
        public string FileFullPath { get; set; }

        private JsonObject deviceStore;
        private JsonObject storedDevices;
        private readonly Dictionary<string, ConnectableDevice> activeDevices = new Dictionary<string, ConnectableDevice>();

        private bool waitToWrite;

        public DefaultConnectableDeviceStore(string fileFullPath)
        {
            FileFullPath = fileFullPath;
            Load();
        }

        public void AddDevice(ConnectableDevice device)
        {
            if (device == null || device.GetServices().Count == 0)
                return;

            if (!activeDevices.ContainsKey(device.Id))
                activeDevices.Add(device.Id, device);

            if (storedDevices == null)
            {
                storedDevices = new JsonObject();
            }
            var storedDevice = storedDevices.Keys.Contains(device.Id) ? storedDevices.GetNamedObject(device.Id) : null;

            if (storedDevice != null)
            {
                UpdateDevice(device);
            }
            else
            {
                storedDevices.Add(device.Id, device.ToJsonObject());
                Store();
            }
        }

        public void RemoveDevice(ConnectableDevice device)
        {
            if (device == null)
                return;

            activeDevices.Remove(device.Id);
            storedDevices.Remove(device.Id);

            Store();
        }

        public void UpdateDevice(ConnectableDevice device)
        {
            if (device == null || device.GetServices().Count == 0)
                return;

            var storedDevice = GetStoredDevice(device.Id);

            if (storedDevice == null)
                return;

            storedDevice.SetNamedValue(ConnectableDevice.KEY_LAST_IP, JsonValue.CreateStringValue(device.LastKnownIpAddress));
            storedDevice.SetNamedValue(ConnectableDevice.KEY_LAST_SEEN, JsonValue.CreateStringValue(device.LastSeenOnWifi));
            storedDevice.SetNamedValue(ConnectableDevice.KEY_LAST_CONNECTED, JsonValue.CreateNumberValue(device.LastConnected));
            storedDevice.SetNamedValue(ConnectableDevice.KEY_LAST_DETECTED, JsonValue.CreateNumberValue(device.LastDetection));

            var services = storedDevice.GetNamedObject(ConnectableDevice.KEY_SERVICES) ?? new JsonObject();

            foreach (var service in device.GetServices())
            {
                var serviceInfo = service.toJSONObject();

                if (serviceInfo != null)
                    services.SetNamedValue(service.ServiceDescription.Uuid, serviceInfo);
            }

            storedDevice.SetNamedValue(ConnectableDevice.KEY_SERVICES, services);

            storedDevices.SetNamedValue(device.Id, storedDevice);
            if (activeDevices.ContainsKey(device.Id))
                activeDevices.Remove(device.Id);
            activeDevices.Add(device.Id, device);

            Store();
        }

        public void RemoveAll()
        {
            activeDevices.Clear();
            storedDevices = new JsonObject();

            Store();
        }

        public JsonObject GetStoredDevices()
        {
            return storedDevices;
        }

        public ConnectableDevice GetDevice(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return null;

            var foundDevice = GetActiveDevice(uuid);

            if (foundDevice == null)
            {
                var foundDeviceInfo = GetStoredDevice(uuid);

                if (foundDeviceInfo != null)
                    foundDevice = new ConnectableDevice(foundDeviceInfo);
            }

            return foundDevice;
        }

        private ConnectableDevice GetActiveDevice(string uuid)
        {
            var foundDevice = activeDevices.ContainsKey(uuid) ? activeDevices[uuid] : null;

            if (foundDevice == null)
            {
                if (activeDevices.Values.SelectMany(device => device.GetServices()).Any(service => uuid.Equals(service.ServiceDescription.Uuid)))
                {
                    return foundDevice;
                }
            }
            return foundDevice;
        }

        private JsonObject GetStoredDevice(string uuid)
        {
            var foundDevice = storedDevices.GetNamedObject(uuid, null);

            if (foundDevice != null) return foundDevice;

            return (from pair in storedDevices select storedDevices.GetNamedObject(pair.Key) into device let services = device.GetNamedObject(ConnectableDevice.KEY_SERVICES) where services != null && services.ContainsKey(uuid) select device).FirstOrDefault();
        }

        public ServiceConfig GetServiceConfig(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return null;

            var device = GetStoredDevice(uuid);
            if (device == null) return null;
            var services = device.GetNamedObject(ConnectableDevice.KEY_SERVICES);

            if (services == null) return null;
            var service = services.GetNamedObject(uuid);

            if (service == null) return null;
            var serviceConfigInfo = service.GetNamedObject(DeviceService.KEY_CONFIG);

            return serviceConfigInfo != null ? ServiceConfig.GetConfig(serviceConfigInfo) : null;
        }

        private void Load()
        {
            Version = CURRENT_VERSION;

            Created = Util.GetTime();
            Updated = Util.GetTime();

            storedDevices = new JsonObject();
        }

        private void Store()
        {
            Updated = Util.GetTime();

            deviceStore = new JsonObject
            {
                {KEY_VERSION, JsonValue.CreateNumberValue(Version)},
                {KEY_CREATED, JsonValue.CreateNumberValue(Created)},
                {KEY_UPDATED, JsonValue.CreateNumberValue(Updated)},
                {KEY_DEVICES, storedDevices}
            };

            if (!waitToWrite)
                WriteStoreToDisk();
        }

        private void WriteStoreToDisk()
        {
            waitToWrite = true;
            Storage.Current.AddOrUpdateValue(Storage.StoredDevicesKeyName, deviceStore.Stringify());
        }
    }
}

