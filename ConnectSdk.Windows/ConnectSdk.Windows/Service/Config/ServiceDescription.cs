using System;
using System.Collections.Generic;
using Windows.Data.Json;

namespace MyRemote.ConnectSDK.Service.Config
{
    public class ServiceDescription
    {

        public static string KeyFilter = "filter";
        public static string KeyIpAddress = "ipAddress";
        public static string KeyUuid = "uuid";
        public static string KeyFriendly = "friendlyName";
        public static string KeyModelName = "modelName";
        public static string KeyModelNumber = "modelNumber";
        public static string KeyPort = "port";
        public static string KeyVersion = "version";
        public static string KeyServiceId = "serviceId";

        public ServiceDescription()
        {
            LastDetection = double.MaxValue;
        }

        public ServiceDescription(string serviceFilter, string uuid, string ipAddress)
        {
            LastDetection = double.MaxValue;
            if (uuid == null) throw new ArgumentNullException("uuid");
            ServiceFilter = serviceFilter;
            Uuid = uuid;
            IpAddress = ipAddress;
        }

        public ServiceDescription(JsonObject json)
        {
            LastDetection = double.MaxValue;
            ServiceFilter = json.GetNamedString(KeyFilter);
            IpAddress = json.GetNamedString(KeyIpAddress);
            Uuid = json.GetNamedString(KeyUuid);
            FriendlyName = json.GetNamedString(KeyFriendly);
            ModelName = json.GetNamedString(KeyModelName);
            ModelNumber = json.GetNamedString(KeyModelNumber);
            Port = (int)json.GetNamedNumber(KeyPort);
            Version = json.GetNamedString(KeyVersion,"");
            ServiceId = json.GetNamedString(KeyServiceId,"");
        }

        public string Uuid { get; set; }

        public string IpAddress { get; set; }

        public string FriendlyName { get; set; }

        public string ModelName { get; set; }

        public string ModelNumber { get; set; }

        public string Manufacturer { get; set; }

        public string ModelDescription { get; set; }

        public string ServiceFilter { get; set; }

        public int Port { get; set; }

        public string ApplicationUrl { get; set; }

        public string Version { get; set; }

        public List<ConnectSdk.Windows.Core.Upnp.Service.Service> ServiceList { get; set; }

        public string LocationXml { get; set; }

        public Dictionary<string, List<string>> ResponseHeaders { get; set; }

        public string ServiceId { get; set; }

        public double LastDetection { get; set; }

        public static ServiceDescription GetDescription(JsonObject json)
        {
            return new ServiceDescription(json);
        }

        public JsonObject ToJsonObject()
        {
            var jsonObj = new JsonObject();

            try
            {
                jsonObj.Add(KeyFilter, JsonValue.CreateStringValue(ServiceFilter));
                jsonObj.Add(KeyIpAddress, JsonValue.CreateStringValue(IpAddress));
                jsonObj.Add(KeyUuid, JsonValue.CreateStringValue(Uuid));
                jsonObj.Add(KeyFriendly, JsonValue.CreateStringValue(FriendlyName));
                jsonObj.Add(KeyModelName, JsonValue.CreateStringValue(ModelName));
                jsonObj.Add(KeyModelNumber, JsonValue.CreateStringValue(ModelNumber));
                jsonObj.Add(KeyPort, JsonValue.CreateNumberValue(Port));
                jsonObj.Add(KeyVersion, JsonValue.CreateStringValue(Version));
                jsonObj.Add(KeyServiceId, JsonValue.CreateStringValue(ServiceId));
            }
            catch
            {
            }

            return jsonObj;
        }
    }
}