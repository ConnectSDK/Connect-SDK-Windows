#region Copyright Notice
/*
 * ConnectSdk.Windows
 * SsdpDevice.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 16-4-2015,
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
using System.IO;
using System.Linq;
using System.Xml;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Wrappers;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class SsdpDevice
    {
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

        public static string TagSecCapability = "sec:Capability";
        public static string TagPort = "port";
        public static string TagLocation = "location";

        /// <summary>
        /// Required. UPnP device type.
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Required. Short description for end user.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Required. Manufacturer's name.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Optional. Web site for manufacturer.
        /// </summary>
        public string ManufacturerUrl { get; set; }

        /// <summary>
        /// Recommended. Long description for end user.
        /// </summary>
        public string ModelDescription { get; set; }

        /// <summary>
        /// Required. Model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Recommended. Model number.
        /// </summary>
        public string ModelNumber { get; set; }

        /// <summary>
        /// Optional. Web site for model.
        /// </summary>
        public string ModelUrl { get; set; }

        /// <summary>
        /// Recommended. Serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Required. Unique Device Name.
        /// </summary>
        public string Udn { get; set; }

        /// <summary>
        /// Optional. Universal Product Code.
        /// </summary>
        public string Upc { get; set; }

        public List<Icon> IconList { get; set; }

        public string LocationXml { get; set; }

        public List<Service> ServiceList { get; set; }

        public string St { get; set; }

        public string ApplicationUrl { get; set; }

        public string ServiceUri { get; set; }

        public string BaseUrl { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public string Uuid { get; set; }

        public Dictionary<string, List<string>> Headers { get; set; }

        public SsdpDevice(String url, string st)
            : this(new Uri(url), st)
        {
        }

        public SsdpDevice(Uri urlObject, string st)
        {
            IconList = new List<Icon>();
            ServiceList = new List<Service>();
            BaseUrl = urlObject.Port == -1 ? String.Format("{0}://{1}", urlObject.Scheme, urlObject.Host) : String.Format("{0}://{1}:{2}", urlObject.Scheme, urlObject.Host, urlObject.Port);
            IpAddress = urlObject.Host;
            Port = urlObject.Port;
            Uuid = null;

            ServiceUri = String.Format("{0}://{1}", urlObject.Scheme, urlObject.Host);

            Parse(urlObject);
        }

        public void Parse(Uri url)
        {
            var cl = new HttpClientWrapper();
            var response = cl.GetAsync(url.AbsoluteUri);

            var content = response.Content.ReadAsStreamAsync().Result;
            var description = new StreamReader(content).ReadToEnd();
            LocationXml = description;
            var reader = Util.GenerateStreamFromstring(description);
            var xmlReader = XmlReader.Create(reader);

            while (!xmlReader.EOF)
            {
                var hasRead = false;
                if (xmlReader.Name == TagDeviceType)
                    DeviceType = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagFriendlyName)
                    FriendlyName = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagModelNumber)
                    ModelNumber = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagManufacturer)
                    Manufacturer = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagManufacturerUrl)
                    ManufacturerUrl = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagModelDescription)
                    ModelDescription = xmlReader.ReadElementContentAsString(out hasRead);

                if (xmlReader.Name == TagModelName)
                    ModelName = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagModelUrl)
                    ModelUrl = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagSerialNumber)
                    SerialNumber = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagUdn)
                    Udn = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == TagUpc)
                    Upc = xmlReader.ReadElementContentAsString(out hasRead);

                if (xmlReader.Name == "icon" && xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (IconList == null)
                        IconList = new List<Icon>();
                    IconList.Add(new Icon());
                }

                if (xmlReader.Name == Icon.TagMimeType)
                    IconList[IconList.Count - 1].MimeType = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Icon.TagWidth)
                    IconList[IconList.Count - 1].Width = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Icon.TagHeight)
                    IconList[IconList.Count - 1].Height = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Icon.TagDepth)
                    IconList[IconList.Count - 1].Depth = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Icon.TagUrl)
                    IconList[IconList.Count - 1].Url = xmlReader.ReadElementContentAsString(out hasRead);

                if (xmlReader.Name == "service" && xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (ServiceList == null)
                        ServiceList = new List<Service>();
                    ServiceList.Add(new Service());
                    ServiceList[ServiceList.Count - 1].BaseUrl = BaseUrl;

                }

                if (xmlReader.Name == Service.TagServiceType)
                    ServiceList[ServiceList.Count - 1].ServiceType = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Service.TagServiceId)
                    ServiceList[ServiceList.Count - 1].ServiceId = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Service.TagScpdUrl)
                    ServiceList[ServiceList.Count - 1].ScpdUrl = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Service.TagControlUrl)
                    ServiceList[ServiceList.Count - 1].ControlUrl = xmlReader.ReadElementContentAsString(out hasRead);
                if (xmlReader.Name == Service.TagEventsubUrl)
                    ServiceList[ServiceList.Count - 1].EventSubUrl = xmlReader.ReadElementContentAsString(out hasRead);

                if (!hasRead) xmlReader.Read();
            }
            //SAXParserFactory factory = SAXParserFactory.newInstance();
            //SAXParser saxParser;

            //SSDPDeviceDescriptionParser parser = new SSDPDeviceDescriptionParser(this);

            //URLConnection urlConnection = url.openConnection();

            //applicationURL = urlConnection.getHeaderField("Application-URL");
            //if (applicationURL != null && !applicationURL.substring(applicationURL.length() - 1).equals("/")) {
            //    applicationURL = applicationURL.concat("/");
            //}

            //InputStream in = new BufferedInputStream(urlConnection.getInputStream());
            //Scanner s = null;
            //try {
            //    s = new Scanner(in).useDelimiter("\\A");
            //    locationXML = s.hasNext() ? s.next() : "";

            //    saxParser = factory.newSAXParser();
            //    saxParser.parse(new ByteArrayInputStream(locationXML.getBytes()), parser);
            //} finally {
            //    in.close();
            //    if (s != null)
            //        s.close();
            //}

            if (response.Headers.Count() >0)
                Headers = new Dictionary<string, List<string>>();
            foreach (var httpResponseHeader in response.Headers)
            {
                Headers.Add(httpResponseHeader.Key, httpResponseHeader.Value.ToList());
            }
        }


        public override string ToString()
        {
            return FriendlyName;
        }
    }

    public static class XmlReaderExtensions
    {
        public static string ReadElementContentAsString(this XmlReader str, out bool changed)
        {
            var value = str.ReadElementContentAsString();
            changed = true;
            return value;
        }
    }
}