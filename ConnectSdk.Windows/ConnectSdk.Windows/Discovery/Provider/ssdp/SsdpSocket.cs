#region Copyright Notice
/*
 * ConnectSdk.Windows
 * SsdpSocket.cs
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
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using ConnectSdk.Windows.Core.Upnp.Ssdp;
using ConnectSdk.Windows.Etc.Helper;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class SsdpSocket
    {
        public event EventHandler<string> MessageReceivedChanged;
        public event EventHandler<string> NotifyReceivedChanged;
        public delegate void EventHandler(object sender, EventArgs e);

        private DatagramSocket socket;

        public bool IsConnected { get; private set; }

        /// <summary>
        /// Used to send SSDP packet
        /// </summary>
        /// <param name="data">The SSDP packet</param>
        /// <returns>unused</returns>
        public async Task<int> Send(string data)
        {

            socket = new DatagramSocket();
            var profile = NetworkInformation.GetInternetConnectionProfile();


            socket.MessageReceived += (sender, args) =>
            {
                var reader = new StreamReader(args.GetDataStream().AsStreamForRead());
                {
                    string response = reader.ReadToEndAsync().Result;
                    OnMessageReceived(new MessageReceivedArgs(response));
                }
            };

            try
            {
                socket.BindServiceNameAsync("", profile.NetworkAdapter);
            }
            catch (Exception e)
            {
                Logger.Current.AddMessage("There was an error binding the multicast socket: " + e.Message);
            }

            var remoteHost = new global::Windows.Networking.HostName(SSDP.Address);
            var reqBuff = Encoding.UTF8.GetBytes(data);

            var stream = await socket.GetOutputStreamAsync(remoteHost, SSDP.Port.ToString());
            await stream.WriteAsync(reqBuff.AsBuffer());

            if (IsConnected) return 0;

            socket.JoinMulticastGroup(remoteHost);
            IsConnected = !IsConnected;

            return 0;

        }

        protected virtual void OnMessageReceived(MessageReceivedArgs e)
        {
            if (MessageReceivedChanged != null)
                MessageReceivedChanged(this, e.Message);
        }

        protected virtual void OnNotifyReceived(MessageReceivedArgs e)
        {
            if (NotifyReceivedChanged != null)
                NotifyReceivedChanged(this, e.Message);
        }

        public void Close()
        {
            if (socket != null)
            {
                socket.Dispose();
            }
        }
    }
}