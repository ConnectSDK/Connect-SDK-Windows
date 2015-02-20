using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Config;

namespace ConnectSdk.Windows.Device
{
    public class DefaultConnectableDeviceStore : IConnectableDeviceStore
    {
        private const int CurrentVersion = 0;

        public static string KeyVersion = "version";
        public static string KeyCreated = "created";
        public static string KeyUpdated = "updated";
        public static string KeyDevices = "devices";

        // ReSharper disable UnusedField.Compiler
        private static string dirpath = "/android/data/connect_sdk/";
        private static string filename = "StoredDevices";

        private static string ipAddress = "ipAddress";
        private static string friendlyName = "friendlyName";
        private static string modelName = "modelName";
        private static string modelNumber = "modelNumber";
        private static string services = "services";
        private static string description = "description";
        private static string config = "config";

        private static string filter = "filter";
        private static string uuid = "uuid";
        private static string port = "port";

        private static string serviceUuid = "serviceUUID";
        private static string clientKey = "clientKey";
        private static string serverCertificate = "serverCertificate";
        private static string pairingKey = "pairingKey";

        private static string defaultServiceWebostv = "WebOSTVService";
        private static string defaultServiceNetcasttv = "NetcastTVService";

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

        public DefaultConnectableDeviceStore()
        {
            //String dirPath;
            //if (Environment. .getExternalStorageState().equals(Environment.MEDIA_MOUNTED))
            //{
            //    dirPath = Environment.getExternalStorageDirectory().getAbsolutePath();
            //}
            //else
            //{
            //    dirPath = Environment.MEDIA_UNMOUNTED;
            //}
            //fileFullPath = dirPath + DIRPATH + FILENAME;
            //global::Windows.Storage.ApplicationData settings;


            //try
            //{
            //    fileFullPath = context.getPackageManager().getPackageInfo(context.getPackageName(), 0).applicationInfo.dataDir + "/" + FILENAME;
            //}
            //catch (NameNotFoundException e)
            //{
            //    e.printStackTrace();
            //}

            Load();
        }

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

            storedDevice.SetNamedValue(ConnectableDevice.KeyLastIp, JsonValue.CreateStringValue(device.LastKnownIpAddress));
            if (device.LastSeenOnWifi != null)
                storedDevice.SetNamedValue(ConnectableDevice.KeyLastSeen, JsonValue.CreateStringValue(device.LastSeenOnWifi));
            if (device.LastConnected > 0)
                storedDevice.SetNamedValue(ConnectableDevice.KeyLastConnected, JsonValue.CreateNumberValue(device.LastConnected));
            if (device.LastDetection > 0)
                storedDevice.SetNamedValue(ConnectableDevice.KeyLastDetected, JsonValue.CreateNumberValue(device.LastDetection));

            var services = storedDevice.GetNamedObject(ConnectableDevice.KeyServices) ?? new JsonObject();

            foreach (var service in device.GetServices())
            {
                var serviceInfo = service.ToJsonObject();

                if (serviceInfo != null)
                    services.SetNamedValue(service.ServiceDescription.Uuid, serviceInfo);
            }

            storedDevice.SetNamedValue(ConnectableDevice.KeyServices, services);

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

            return (from pair in storedDevices select storedDevices.GetNamedObject(pair.Key) into device let services = device.GetNamedObject(ConnectableDevice.KeyServices) where services != null && services.ContainsKey(uuid) select device).FirstOrDefault();
        }

        public ServiceConfig GetServiceConfig(string uuid)
        {
            if (string.IsNullOrEmpty(uuid))
                return null;

            var device = GetStoredDevice(uuid);
            if (device == null) return null;
            var services = device.GetNamedObject(ConnectableDevice.KeyServices);

            if (services == null) return null;
            var service = services.GetNamedObject(uuid);

            if (service == null) return null;
            var serviceConfigInfo = service.GetNamedObject(DeviceService.KEY_CONFIG);

            return serviceConfigInfo != null ? ServiceConfig.GetConfig(serviceConfigInfo) : null;
        }

        private void Load()
        {
            Version = CurrentVersion;

            Created = Util.GetTime();
            Updated = Util.GetTime();

            storedDevices = new JsonObject();

            var value = Storage.Current.GetValueOrDefault(Storage.StoredDevicesKeyName, string.Empty);
            JsonObject data;
            if (!JsonObject.TryParse(value, out data)) return;

            storedDevices = data.GetNamedObject(KeyDevices, null) ?? new JsonObject();

            Version = (int)data.GetNamedNumber(KeyVersion, CurrentVersion);
            Created = (long)data.GetNamedNumber(KeyCreated, 0);
            Updated = (long)data.GetNamedNumber(KeyUpdated, 0);
        }

        private void Store()
        {
            Updated = Util.GetTime();

            deviceStore = new JsonObject
            {
                {KeyVersion, JsonValue.CreateNumberValue(Version)},
                {KeyCreated, JsonValue.CreateNumberValue(Created)},
                {KeyUpdated, JsonValue.CreateNumberValue(Updated)},
                {KeyDevices, storedDevices}
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

