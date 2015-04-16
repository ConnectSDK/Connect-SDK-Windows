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

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class SsdpDevice
    {
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

        public SsdpDevice(String url)
            : this(new Uri(url))
        {
        }

        public SsdpDevice(Uri urlObject)
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

            //headers = urlConnection.getHeaderFields();
        }


        public override string ToString()
        {
            return FriendlyName;
        }
    }
}