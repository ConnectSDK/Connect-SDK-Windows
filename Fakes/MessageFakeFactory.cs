#region Copyright Notice
/*
 * ConnectSdk.Windows
 * MessageFakeFactory.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 6-5-2015,
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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace ConnectSdk.Windows.Fakes
{
    public class MessageFakeFactory
    {
        public static MessageFakeFactory Instance;

        public event EventHandler<string> NewDatagraMessage;

        /// <summary>
        /// Instructs the factory to spam with messages as they would have been received by a normal datagram multicast socket
        /// </summary>
        public async void StartJoinMulticastGroup()
        {
            var messages = new List<string>();
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Fakes/SSDP.txt"));
            string ssdpMessages;

            using (StreamReader stream = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                ssdpMessages = await stream.ReadToEndAsync();
                var lines = Regex.Split(ssdpMessages, "\r\n|\r|\n");

                var message = "";
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (lines[i].Length > 0)
                        message += lines[i] + "\r\n";
                    else
                    {
                        messages.Add(message);
                        message = lines[i];
                    }
                }
            }

            for (int i = 0; i < messages.Count; i++)
            {
                if (NewDatagraMessage != null)
                    NewDatagraMessage(this, messages[i]);
            }
        }

        public async Task<HttpResponseMessage> GetResponseMessage(string url)
        {
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Fakes/LGWebOSLocation.xml"));
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Fakes/NetcastLocation.xml"));
            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Fakes/XboxLocation.xml"));
            string locationXMLMessage;

            using (StreamReader stream = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                locationXMLMessage = await stream.ReadToEndAsync();
            }

            HttpResponseMessage message = new HttpResponseMessage();
            message.Headers.Add("DLNADeviceName.lge.com", "LG%20Smart%2b%20TV");
            message.Headers.Add("Date", "Tue, 05 May 2015 17:31:30 GMT");
            try
            {
                message.Headers.TryAddWithoutValidation("Server", @"Linux/i686 UPnP/1,0 DLNADOC/1.50 LGE WebOS TV/Version 0.9");
            }
            catch
            {
                
            }
            message.Content = new StringContent(locationXMLMessage);
            return message;
        }

        public static void Start()
        {
            Instance = new MessageFakeFactory();
        }
    }
}
