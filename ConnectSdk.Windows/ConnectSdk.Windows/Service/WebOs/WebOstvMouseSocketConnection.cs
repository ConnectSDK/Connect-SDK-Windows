#region Copyright Notice
/*
 * ConnectSdk.Windows
 * WebOstvMouseSocketConnection.cs
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
using System.Diagnostics;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ConnectSdk.Windows.Service.WebOs
{
    public class WebOstvMouseSocketConnection
    {
        MessageWebSocket ws;
        readonly String socketPath;
        private bool isConnected;

        public WebOstvMouseSocketConnection(String socketPath)
        {
            this.socketPath = socketPath.StartsWith("wss:") ? socketPath.Replace("wss:", "ws:").Replace(":3001/", ":3000/") : socketPath;

            CreateSocket();
            //try
            //{
            //    var uri = new Uri(this.socketPath);
            //    ConnectPointer(uri);
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
        }

        private void CreateSocket()
        {
            ws = new MessageWebSocket();
            ws.Control.MessageType = SocketMessageType.Utf8;
            ws.MessageReceived += (sender, args) =>
            {
                string read;

                using (var reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = global::Windows.Storage.Streams.UnicodeEncoding.Utf8;

                    read = reader.ReadString(reader.UnconsumedBufferLength);
                    Debug.WriteLine("{0} : {1} : {2}", DateTime.Now, "received", read);
                }
                OnMessage(read);
            };
            ws.Closed += (sender, args) =>
            {
            };
        }

        public void OnMessage(String data)
        {
            //this.handleMessage(data);
        }

        public async void Connect()
        {
            try
            {
                await ws.ConnectAsync(new Uri(socketPath));
                isConnected = true;
            }
            catch (Exception ex)
            {
                // ReSharper disable once UnusedVariable
                var status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                isConnected = false;
                
            }
        }

        public void Disconnect()
        {
            if (ws != null)
            {
                isConnected = false;
                ws.Dispose();
                ws = null;
            }
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public void Click()
        {
            if (IsConnected())
            {
                var sb = new StringBuilder();

                sb.Append("type:click\n");
                sb.Append("\n");

                Send(sb.ToString());
            }
        }

        public void Button(ButtonType type)
        {
            String keyName;
            switch (type)
            {
                case ButtonType.Home:
                    keyName = "HOME";
                    break;
                case ButtonType.Back:
                    keyName = "BACK";
                    break;
                case ButtonType.Up:
                    keyName = "UP";
                    break;
                case ButtonType.Down:
                    keyName = "DOWN";
                    break;
                case ButtonType.Left:
                    keyName = "LEFT";
                    break;
                case ButtonType.Right:
                    keyName = "RIGHT";
                    break;

                default:
                    keyName = "NONE";
                    break;
            }

            Button(keyName);
        }

        public void Button(String keyName)
        {
            if (keyName != null)
            {
                if (keyName.Equals("HOME")
                    || keyName.Equals("BACK")
                    || keyName.Equals("UP")
                    || keyName.Equals("DOWN")
                    || keyName.Equals("LEFT")
                    || keyName.Equals("RIGHT")
                    || keyName.Equals("3D_MODE"))
                {

                    SendSpecialKey(keyName);
                }
            }
        }

        private void SendSpecialKey(String keyName)
        {
            if (!IsConnected())
                Connect();
            {
                var sb = new StringBuilder();

                sb.Append("type:button\n");
                sb.Append("name:" + keyName + "\n");
                sb.Append("\n");

                Send(sb.ToString());
            }
        }

        private DataWriter messageWriter;

        private void Send(string sb)
        {
            if (isConnected)
            {
                ws.Control.MessageType = SocketMessageType.Utf8;
                ws.OutputStream.FlushAsync().GetResults();
                if (messageWriter == null)
                    messageWriter = new DataWriter(ws.OutputStream);
                messageWriter.WriteString(sb);
                messageWriter.StoreAsync();
                Debug.WriteLine("{0} : {1} : {2}", DateTime.Now, "sent", sb);
            }
        }

        public void Move(double dx, double dy)
        {
            if (!IsConnected())
                Connect();

            if (IsConnected())
            {
                var sb = new StringBuilder();

                sb.Append("type:move\n");
                sb.Append("dx:" + dx + "\n");
                sb.Append("dy:" + dy + "\n");
                sb.Append("down:0\n");
                sb.Append("\n");

                Send(sb.ToString());
            }
        }

        public void Move(double dx, double dy, bool drag)
        {
            if (IsConnected())
            {
                var sb = new StringBuilder();

                sb.Append("type:move\n");
                sb.Append("dx:" + dx + "\n");
                sb.Append("dy:" + dy + "\n");
                sb.Append("down:" + (drag ? 1 : 0) + "\n");
                sb.Append("\n");

                Send(sb.ToString());
            }
        }

        public void Scroll(double dx, double dy)
        {
            if (IsConnected())
            {
                var sb = new StringBuilder();

                sb.Append("type:scroll\n");
                sb.Append("dx:" + dx + "\n");
                sb.Append("dy:" + dy + "\n");
                sb.Append("\n");

                Send(sb.ToString());
            }
        }
    }
}