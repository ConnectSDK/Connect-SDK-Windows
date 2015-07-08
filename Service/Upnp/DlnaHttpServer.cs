#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DlnaHttpServer.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 11-6-2015,
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Upnp
{
    public class DlnaHttpServer : IDisposable
    {
        public int Port = 49291;
        private const uint BufferSize = 8192;

        private StreamSocketListener listener;
        private List<UrlServiceSubscription> subscriptions;

        public DlnaHttpServer()
        {
            Subscriptions = new List<UrlServiceSubscription>();
        }

        public bool IsRunning { get; set; }

        public List<UrlServiceSubscription> Subscriptions
        {
            get { return subscriptions; }
            set { subscriptions = value; }
        }

        public void Dispose()
        {
            listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            var body = "";

            var request = new StringBuilder();
            using (var input = socket.InputStream)
            {
                var data = new byte[BufferSize];
                var buffer = data.AsBuffer();
                var dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    body = Encoding.UTF8.GetString(data, 0, data.Length);
                    request.Append(body);
                    dataRead = buffer.Length;
                }
            }

            using (socket.OutputStream)
            {
                DataWriter dr = null;
                try
                {
                    dr = new DataWriter(socket.OutputStream);
                    var message = new StringBuilder();
                    message.AppendLine("HTTP/1.1 200 OK");
                    message.AppendLine("Connection: Close");
                    message.AppendLine("Content-Length: 0");
                    dr.WriteString(message.ToString());
                    dr.StoreAsync();
                }
                catch
                {

                }
                finally
                {
                    if (dr != null)
                    {
                        try
                        {
                            dr.DetachStream();
                        }
                        catch
                        {
                        }
                        dr.Dispose();
                    }
                }
            }

            if (body == null) return;

            var parser = new DlnaNotifyParser();
            try
            {
                var propertySet = parser.Parse(body);
                var lastChange =
                    (from p in propertySet let jsonObject = p as JsonObject where jsonObject != null && jsonObject.ContainsKey("LastChange") select (p as JsonObject).GetNamedObject("LastChange")).FirstOrDefault();
                if (lastChange != null)
                {
                    HandleLastChange(lastChange);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void HandleLastChange(JsonObject lastChange)
        {
            if (lastChange.ContainsKey("InstanceID"))
            {
                var instanceIds = lastChange.GetNamedArray("InstanceID");

                for (int i = 0; i < instanceIds.Count; i++)
                {
                    var events = instanceIds[i] as JsonArray;

                    for (var j = 0; j < events.Count; j++)
                    {
                        var entry = events[i] as JsonObject;
                        HandleEntry(entry);
                    }
                }
            }
        }

        private void HandleEntry(JsonObject entry)
        {
            if (entry.ContainsKey("TransportState"))
            {
                String transportState = entry.GetNamedString("TransportState");
                var status = (PlayStateStatus)Enum.Parse(typeof(PlayStateStatus), transportState);

                foreach (var sub in subscriptions.Where(sub => sub.Target.Equals("playState", StringComparison.OrdinalIgnoreCase)))
                {
                    for (var j = 0; j < sub.GetListeners().Count; j++)
                    {

                        var responseListener = sub.GetListeners()[j];
                        Util.PostSuccess(responseListener, status);
                    }
                }
            }

            if ((entry.ContainsKey("Volume") && !entry.ContainsKey("channel")) || 
                (entry.ContainsKey("Volume") && entry.GetNamedString("channel").Equals("Master")))
            {
                var intVolume = int.Parse(entry.GetNamedString("Volume"));
                var volume = (float) intVolume/100;

                foreach (var sub in subscriptions.Where(sub => sub.Target.Equals("volume", StringComparison.OrdinalIgnoreCase)))
                {
                    for (var j = 0; j < sub.GetListeners().Count; j++)
                    {
                        var responseListener = sub.GetListeners()[j];
                        Util.PostSuccess(responseListener, volume);
                    }
                }
            }


            if ((entry.ContainsKey("Mute") && !entry.ContainsKey("channel")) ||
                (entry.ContainsKey("Mute") && entry.GetNamedString("channel").Equals("Master")))
            {
                var muteStatus = entry.GetNamedString("Mute");
                bool mute;
                try
                {
                    mute = (int.Parse(muteStatus)) == 1;
                }
                catch (Exception)
                {
                    mute = bool.Parse(muteStatus);
                }

                foreach (var sub in subscriptions.Where(sub => sub.Target.Equals("mute", StringComparison.OrdinalIgnoreCase)))
                {
                    for (var j = 0; j < sub.GetListeners().Count; j++)
                    {
                        var responseListener = sub.GetListeners()[j];
                        Util.PostSuccess(responseListener, mute);
                    }
                }
            }

            if (entry.ContainsKey("CurrentTrackMetaData"))
            {
                String trackMetaData = entry.GetNamedString("CurrentTrackMetaData");

                MediaInfo info = DlnaMediaInfoParser.GetMediaInfo(trackMetaData);

                foreach (var sub in subscriptions.Where(sub => sub.Target.Equals("info", StringComparison.OrdinalIgnoreCase)))
                {
                    for (var j = 0; j < sub.GetListeners().Count; j++)
                    {

                        var responseListener = sub.GetListeners()[j];
                        Util.PostSuccess(responseListener, info);
                    }
                }
            }
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            listener = new StreamSocketListener();
            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            listener.BindServiceNameAsync(Port.ToString());

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            foreach (UrlServiceSubscription sub in Subscriptions)
            {
                sub.Unsubscribe();
            }
            Subscriptions.Clear();
            IsRunning = false;
        }
    }
}
