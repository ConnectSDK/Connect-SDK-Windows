#region Copyright Notice
/*
 * ConnectSdk.Windows
 * SsdpSocket.cs
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Connectivity;
using ConnectSdk.Windows.Core.Upnp.Ssdp;
using ConnectSdk.Windows.Etc.Helper;
using ConnectSdk.Windows.Wrappers;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class SsdpSocket
    {
        /// <summary>
        /// Event called when a message is received by the socket
        /// </summary>
        public event EventHandler<string> MessageReceivedChanged;

        /// <summary>
        /// Handler for the events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void EventHandler(object sender, EventArgs e);

        private DatagramSocketWrapper socket;


        public bool IsConnected { get; private set; }

        /// <summary>
        /// Used to send SSDP packet
        /// </summary>
        /// <param name="data">The SSDP packet</param>
        /// <returns>unused</returns>
        public async Task<int> Send(string data)
        {

            socket = new DatagramSocketWrapper();
            var profile = NetworkInformation.GetInternetConnectionProfile();


            socket.MessageReceived += (sender, args) =>
            {
                HandleDatagramMessage(args);
            };

            try
            {
                socket.BindServiceNameAsync("", profile.NetworkAdapter);
            }
            catch (Exception e)
            {
                Logger.Current.AddMessage("There was an error binding the multicast socket: " + e.Message);
            }

            var remoteHost = new HostName(SSDP.Address);
            var reqBuff = Encoding.UTF8.GetBytes(data);

            var stream = await socket.GetOutputStreamAsync(remoteHost, SSDP.Port.ToString());
            await stream.WriteAsync(reqBuff.AsBuffer());

            if (IsConnected) return 0;

            socket.JoinMulticastGroup(remoteHost);
            IsConnected = !IsConnected;

            return 0;

        }

        private void HandleDatagramMessage(string message)
        {
            OnMessageReceived(new MessageReceivedArgs(message));
        }

        protected virtual void OnMessageReceived(MessageReceivedArgs e)
        {
            if (MessageReceivedChanged != null)
                MessageReceivedChanged(this, e.Message);
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