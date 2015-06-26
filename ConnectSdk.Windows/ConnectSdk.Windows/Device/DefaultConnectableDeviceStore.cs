#region Copyright Notice
/*
 * ConnectSdk.Windows
 * defaultconnectabledevicestore.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
            Load();
        }

        public DefaultConnectableDeviceStore(string fileFullPath)
        {
            FileFullPath = fileFullPath;
            Load();
        }

        public void AddDevice(ConnectableDevice device)
        {
            if (device == null || device.Services.Count == 0)
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
            if (device == null || device.Services.Count == 0)
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

            var tempServices = storedDevice.GetNamedObject(ConnectableDevice.KeyServices) ?? new JsonObject();

            foreach (var service in device.Services)
            {
                var serviceInfo = service.ToJsonObject();

                if (serviceInfo != null)
                    tempServices.SetNamedValue(service.ServiceDescription.Uuid, serviceInfo);
            }

            storedDevice.SetNamedValue(ConnectableDevice.KeyServices, tempServices);

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

        public ConnectableDevice GetDevice(string uuidParam)
        {
            if (string.IsNullOrEmpty(uuidParam))
                return null;

            var foundDevice = GetActiveDevice();

            if (foundDevice == null)
            {
                var foundDeviceInfo = GetStoredDevice(uuidParam);

                if (foundDeviceInfo != null)
                    foundDevice = new ConnectableDevice(foundDeviceInfo);
            }

            return foundDevice;
        }

        private ConnectableDevice GetActiveDevice()
        {
            var foundDevice = activeDevices.ContainsKey(uuid) ? activeDevices[uuid] : null;

            if (foundDevice == null)
            {
                foreach (var device in activeDevices.Values)
                {
                    foreach (var service in device.Services)
                    {
                        if (uuid.Equals(service.ServiceDescription.Uuid))
                        {
                            return foundDevice;
                        }
                    }
                }
            }
            return foundDevice;


            //var foundDevice = activeDevices.ContainsKey(uuid) ? activeDevices[uuid] : null;

            //if (foundDevice != null) return foundDevice;
            //return activeDevices.Values.SelectMany(device => device.Services).Any(service => uuid.Equals(service.ServiceDescription.Uuid)) ? foundDevice : foundDevice;
        }

        private JsonObject GetStoredDevice(string uuidParam)
        {
            var foundDevice = storedDevices.GetNamedObject(uuidParam, null);

            return foundDevice ??
                   (from
                        pair
                        in storedDevices
                    select storedDevices.GetNamedObject(pair.Key) into device
                    let services = device.GetNamedObject(ConnectableDevice.KeyServices)
                    where services != null && services.ContainsKey(uuidParam)
                    select device
                    ).FirstOrDefault();
        }

        //public ServiceConfig GetServiceConfig(string uuidParam)
        //{
        //    if (string.IsNullOrEmpty(uuidParam))
        //        return null;

        //    var device = GetStoredDevice(uuidParam);
        //    if (device == null) return null;
        //    var tempServices = device.GetNamedObject(ConnectableDevice.KeyServices);

        //    if (tempServices == null) return null;
        //    var service = tempServices.GetNamedObject(uuidParam);

        //    if (service == null) return null;
        //    var serviceConfigInfo = service.GetNamedObject(DeviceService.KEY_CONFIG);

        //    return serviceConfigInfo != null ? ServiceConfig.GetConfig(serviceConfigInfo) : null;
        //}

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


        public ServiceConfig GetServiceConfig(ServiceDescription serviceDescription)
        {
            if (serviceDescription == null) return null;

            var uuidValue = serviceDescription.Uuid;
            if (string.IsNullOrEmpty(uuidValue)) return null;

            var device = GetStoredDevice(uuidValue);

            if (device != null)
            {
                var servicesParam = device.GetNamedObject(ConnectableDevice.KeyServices, null);
                if (servicesParam != null)
                {
                    var service = servicesParam.GetNamedObject(uuidValue, null);
                    if (service != null)
                    {
                        var serviceConfigInfo = service.GetNamedObject(DeviceService.KEY_CONFIG);
                        if (serviceConfigInfo != null)
                            return ServiceConfig.GetConfig(serviceConfigInfo);
                    }
                }
            }
            return null;
        }
    }
}

