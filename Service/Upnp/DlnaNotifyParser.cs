#region Copyright Notice
/*
 * ConnectSdk.Windows
 * dlnanotifyparser.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 5-7-2015,
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
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;

namespace ConnectSdk.Windows.Service.Upnp
{
    public class DlnaNotifyParser
    {
        public JsonArray Parse(string input)
        {
            var res = new JsonArray();

            var lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var xmlContent = "";
            var passedHeaders = false;
            for (var i = 0; i < lines.Count(); i++)
            {
                if (string.IsNullOrEmpty(lines[i].Trim()))
                    passedHeaders = true;
                else
                {
                    if (passedHeaders)
                    {
                        xmlContent += lines[i];
                    }
                }
            }

            try
            {
                // ignore crappy data
                if (xmlContent.StartsWith("\0")) return res;

                xmlContent = System.Net.WebUtility.HtmlDecode(xmlContent);

                //sanitize string to remove non xml characters
                var sanitizedXml = CleanInvalidXmlChars(xmlContent);

                var doc = new XmlDocument();

                var set = new XmlLoadSettings { ValidateOnParse = false };

                doc.LoadXml(sanitizedXml, set);

                if (doc.FirstChild != null)
                {
                    var properties = doc.FirstChild.ChildNodes;

                    //var res = new JsonArray();
                    foreach (var property in properties)
                    {
                        var xmlElement = property as XmlElement;
                        if (xmlElement != null)
                        {

                            var name =
                                (from x in xmlElement.ChildNodes where x is XmlElement select x.NodeName).FirstOrDefault
                                    ();
                            var propertyObject = new JsonObject();
                            if (name.Equals("LastChange"))
                            {
                                var lastChange = (from x in xmlElement.ChildNodes where x is XmlElement select x)
                                        .FirstOrDefault();
                                var eventParser = new DlnaEventParser();
                                var evt = eventParser.Parse(lastChange);
                                propertyObject.Add(name, evt);

                            }
                            else
                            {
                                var value =
                                    (from x in xmlElement.ChildNodes where x is XmlElement select x.InnerText)
                                        .FirstOrDefault();
                                propertyObject.Add(name, JsonValue.CreateStringValue(value));
                            }
                            res.Add(propertyObject);
                        }

                    }
                }
            }
            catch (Exception)
            {


            }
            return res;
        }

        /// <summary>
        /// Method from https://gist.github.com/jokecamp/7529013
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CleanInvalidXmlChars(string text)
        {
            // From xml spec valid chars:
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]    
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF.
            const string re = @"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]";
            return Regex.Replace(text, re, "");
        }
    }

    public class DlnaEventParser
    {
        public JsonObject Parse(IXmlNode lastChange)
        {
            var evt = new JsonObject();

            var instanceIDs = new JsonArray();
            var queueIDs = new JsonArray();

            var eventXml = (from x in lastChange.ChildNodes where x is XmlElement select x)
                                       .FirstOrDefault();

            foreach (IXmlNode n in eventXml.ChildNodes)
            {
                var node = n as XmlElement;
                if (node != null)
                {
                    if (node.NodeName.Equals("InstanceID"))
                    {
                        instanceIDs.Add(ReadInstance(node));
                    }
                    else if (node.NodeName.Equals("QueueID"))
                    {
                        queueIDs.Add(ReadQueue(node));
                    }
                }
            }

            if (instanceIDs.Count > 0)
                evt.Add("InstanceID", instanceIDs);

            if (queueIDs.Count > 0)
                evt.Add("QueueID", queueIDs);

            return evt;
        }

        private IJsonValue ReadQueue(XmlElement node)
        {
            var instanceIds = new JsonArray();
            var data = new JsonObject();
            data.Add("value", JsonValue.CreateStringValue(node.GetAttribute("val")));
            instanceIds.Add(data);
            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                var n = node.ChildNodes[i] as XmlElement;
                if (n != null)
                    instanceIds.Add(ReadEntry(n));
            }
            return instanceIds;
        }

        private JsonObject ReadEntry(XmlElement xmlElement)
        {
            var value = xmlElement.GetAttribute("val");
            
            var channel = xmlElement.GetAttribute("channel");

            var data = new JsonObject();
            data.Add(xmlElement.NodeName, JsonValue.CreateStringValue(value));

            if (!string.IsNullOrEmpty(channel))
                data.Add("channel", JsonValue.CreateStringValue(channel));

            return data;
        }

        private IJsonValue ReadInstance(XmlElement node)
        {
            var instanceIds = new JsonArray();
            var data = new JsonObject();
            data.Add("value", JsonValue.CreateStringValue(node.GetAttribute("val")));
            instanceIds.Add(data);
            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                var n = node.ChildNodes[i] as XmlElement;
                if (n != null)
                    instanceIds.Add(ReadEntry(n));
            }
            return instanceIds;
        }
    }
}