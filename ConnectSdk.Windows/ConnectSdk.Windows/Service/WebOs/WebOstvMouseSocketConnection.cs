using System;
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
            if (socketPath.StartsWith("wss:"))
            {
                this.socketPath = socketPath.Replace("wss:", "ws:").Replace(":3001/", ":3000/"); // downgrade to plaintext
            }
            else
                this.socketPath = socketPath;

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
                throw;
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
            if (messageWriter == null)
                messageWriter = new DataWriter(ws.OutputStream);
            messageWriter.WriteString(sb);
            messageWriter.StoreAsync().GetResults();
        }

        public void Move(double dx, double dy)
        {
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