#region Copyright Notice
/*
 * ConnectSdk.Windows
 * DatagramSocketWrapper.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 8-5-2015,
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
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using ConnectSdk.Windows.Fakes;

namespace ConnectSdk.Windows.Wrappers
{
    public class DatagramSocketWrapper : IDisposable
    {
        private DatagramSocket socket;
        public event EventHandler<string> MessageReceived;

        public IAsyncAction BindServiceNameAsync(string localServiceName, NetworkAdapter adapter)
        {
            return socket.BindServiceNameAsync(localServiceName, adapter);
        }

        public DatagramSocketWrapper()
        {
            socket = new DatagramSocket();
            socket.MessageReceived += SocketOnMessageReceived;

            if (MessageFakeFactory.Instance != null)
            {
                MessageFakeFactory.Instance.NewDatagraMessage += (sender, s) =>
                {
                    if (MessageReceived != null)
                        MessageReceived(this, s);
                };
            }
        }

        private void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            if (MessageReceived != null)
            {
                var reader = new StreamReader(args.GetDataStream().AsStreamForRead());
                {
                    var response = reader.ReadToEndAsync().Result;
                    MessageReceived(this, response);
                }
            }
        }

        public IAsyncOperation<IOutputStream> GetOutputStreamAsync(HostName remoteHostName, string remoteServiceName)
        {
            return socket.GetOutputStreamAsync(remoteHostName, remoteServiceName);
        }

        public void JoinMulticastGroup(HostName host)
        {
            if (MessageFakeFactory.Instance != null)
            {
                MessageFakeFactory.Instance.StartJoinMulticastGroup();
            }
            else
                socket.JoinMulticastGroup(host);
        }

        public void Dispose()
        {
            MessageReceived = null;
            socket = null;
        }
    }
}