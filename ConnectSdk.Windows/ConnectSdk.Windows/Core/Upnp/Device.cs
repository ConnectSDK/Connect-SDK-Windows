﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml;
using ConnectSdk.Windows.Core.Upnp.Ssdp;
using ConnectSdk.Windows.Discovery.Provider.ssdp;

namespace ConnectSdk.Windows.Core.Upnp
{
    public class Device
    {
        public static string Tag = "device";
        public static string TagDeviceType = "deviceType";
        public static string TagFriendlyName = "friendlyName";
        public static string TagManufacturer = "manufacturer";
        public static string TagManufacturerUrl = "manufacturerURL";
        public static string TagModelDescription = "modelDescription";
        public static string TagModelName = "modelName";
        public static string TagModelNumber = "modelNumber";
        public static string TagModelUrl = "modelURL";
        public static string TagSerialNumber = "serialNumber";
        public static string TagUdn = "UDN";
        public static string TagUpc = "UPC";
        public static string TagIconList = "iconList";
        public static string TagServiceList = "serviceList";

        public static string HeaderServer = "Server";

        /// <summary>
        /// Required. UPnP device type. 
        /// </summary>
        public string DeviceType;

        /// <summary>
        /// Required. Short description for end user
        /// </summary>
        public string FriendlyName;

        /// <summary>
        /// Required. Manufacturer's name
        /// </summary>
        public string Manufacturer;

        /// <summary>
        /// Optional. Web site for manufacturer
        /// </summary>
        public string ManufacturerUrl;

        /// <summary>
        /// 
        /// </summary>
        public string ModelDescription;

        /// <summary>
        /// Required. Model name
        /// </summary>
        public string ModelName;

        /// <summary>
        /// Recommended. Model number
        /// </summary>
        public string ModelNumber;

        /// <summary>
        /// Optional. Web site for model
        /// </summary>
        public string ModelUrl;

        /// <summary>
        /// Recommended. Serial number
        /// </summary>
        public string SerialNumber;

        /// <summary>
        /// Required. Unique Device Name
        /// </summary>
        public string Udn;

        /// <summary>
        /// Optional. Universal Product Code.
        /// </summary>
        public string Upc;

        /// <summary>
        /// Required
        /// </summary>
        public List<Icon> IconList = new List<Icon>();

        public string LocationXml;

        /// <summary>
        /// Optional
        /// </summary>
        public List<Discovery.Provider.ssdp.Service> ServiceList = new List<Discovery.Provider.ssdp.Service>();

        public string SearchTarget;
        public string ApplicationUrl;

        public string BaseUrl;
        public string IpAddress;
        public int Port;
        public string Uuid;

        public Dictionary<string, List<string>> Headers;

        public Device(string url, string searchTarget)
        {
            var urlObject = new Uri(url);

            BaseUrl = urlObject.Port == -1 ? string.Format("{0}://{1}", urlObject.Scheme, urlObject.Host) : string.Format("{0}://{1}:{2}", urlObject.Scheme, urlObject.Host, urlObject.Port);
            IpAddress = urlObject.Host;
            Port = urlObject.Port;
            SearchTarget = searchTarget;
            Uuid = null;

            if (searchTarget.Equals("urn:dial-multiscreen-org:service:dial:1", StringComparison.OrdinalIgnoreCase))
                ApplicationUrl = GetApplicationUrl(url);
        }

        public static Device CreateInstanceFromXml(string url, string searchTarget)
        {
            var newDevice = new Device(url, searchTarget);

            var device = newDevice;

            var cl = new HttpClient();
            var response = cl.GetAsync(url).Result;
            if (device.Headers == null)
                device.Headers = new Dictionary<string, List<string>>();
            foreach (var header in response.Headers)
            {
                device.Headers.Add(header.Key, header.Value.ToList());
            }

            var content = response.Content.ReadAsStreamAsync().Result;
            device.LocationXml = new StreamReader(content).ReadToEnd();

            var reader = Util.GenerateStreamFromstring(device.LocationXml);
            var xmlReader = XmlReader.Create(reader);

            while (xmlReader.Read())
            {
                if (xmlReader.Name == TagDeviceType)
                    device.DeviceType = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagFriendlyName)
                    device.FriendlyName = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagModelNumber)
                    device.ModelNumber = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagManufacturer)
                    device.Manufacturer = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagManufacturerUrl)
                    device.ManufacturerUrl = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagModelDescription)
                    device.ModelDescription = xmlReader.ReadElementContentAsString();

                if (xmlReader.Name == TagModelName)
                    device.ModelName = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagModelUrl)
                    device.ModelUrl = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagSerialNumber)
                    device.SerialNumber = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagUdn)
                    device.Udn = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == TagUpc)
                    device.Upc = xmlReader.ReadElementContentAsString();

                if (xmlReader.Name == "icon" && xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (device.IconList == null) 
                        device.IconList = new List<Icon>();
                    device.IconList.Add(new Icon());
                }

                if (xmlReader.Name == Icon.TagMimeType)
                    device.IconList[device.IconList.Count - 1].MimeType = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Icon.TagWidth)
                    device.IconList[device.IconList.Count - 1].Width = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Icon.TagHeight)
                    device.IconList[device.IconList.Count - 1].Height = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Icon.TagDepth)
                    device.IconList[device.IconList.Count - 1].Depth = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Icon.TagUrl)
                    device.IconList[device.IconList.Count - 1].Url = xmlReader.ReadElementContentAsString();

                if (xmlReader.Name == "service" && xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (device.ServiceList == null)
                        device.ServiceList = new List<Discovery.Provider.ssdp.Service>();
                    device.ServiceList.Add(new Discovery.Provider.ssdp.Service());
                }

                if (xmlReader.Name == Discovery.Provider.ssdp.Service.TAG_SERVICE_TYPE)
                    device.ServiceList[device.ServiceList.Count - 1].ServiceType = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Discovery.Provider.ssdp.Service.TAG_SERVICE_ID)
                    device.ServiceList[device.ServiceList.Count - 1].ServiceId = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Discovery.Provider.ssdp.Service.TAG_SCPD_URL)
                    device.ServiceList[device.ServiceList.Count - 1].ScpdUrl = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Discovery.Provider.ssdp.Service.TAG_CONTROL_URL)
                    device.ServiceList[device.ServiceList.Count - 1].ControlUrl = xmlReader.ReadElementContentAsString();
                if (xmlReader.Name == Discovery.Provider.ssdp.Service.TAG_EVENTSUB_URL)
                    device.ServiceList[device.ServiceList.Count - 1].EventSubUrl = xmlReader.ReadElementContentAsString();
            }
            return device;
        }

        private static string GetApplicationUrl(string url)
        {
            var client = new HttpClient();
            var get = new HttpRequestMessage(HttpMethod.Get, url);
            string applicationUrl = null;
            var response = client.SendAsync(get).Result;
            var code = response.StatusCode;
            if (code != HttpStatusCode.OK) return null;
            if (response.Headers.Contains(SSDP.ApplicationUrl))
            {
                applicationUrl = response.Headers.GetValues(SSDP.ApplicationUrl).First();

                if (!applicationUrl.Substring(applicationUrl.Length - 1).Equals("/"))
                {
                    applicationUrl += "/";
                }
            }
            return applicationUrl;
        }

        protected static string ParseUuid(string str)
        {
            const string uuidColon = "uuid:";
            const string colonColon = "::";
            if (str == null)
                return "";

            var start = str.IndexOf(uuidColon, StringComparison.OrdinalIgnoreCase);

            if (start != -1)
            {
                start += uuidColon.Length;
                var end = str.IndexOf(colonColon, StringComparison.OrdinalIgnoreCase);
                return end != -1 ? str.Substring(start, end) : str.Substring(start);
            }
            return str;
        }

        public override string ToString()
        {
            return FriendlyName;
        }
    }
}
