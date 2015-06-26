using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;

namespace ConnectSdk.Windows.Core.Upnp.Ssdp
{
    public class SSDPSocket
    {
        bool socketJoined;
        public event EventHandler<string> MessageReceivedChanged;
        public event EventHandler<string> NotifyReceivedChanged;
        public delegate void EventHandler(object sender, EventArgs e);

        /// <summary>
        /// Used to send SSDP packet
        /// </summary>
        /// <param name="data">The SSDP packet</param>
        /// <returns>unused</returns>
        public async Task<int> Send(string data)
        {

            var socket = new global::Windows.Networking.Sockets.DatagramSocket();
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();

                
                socket.MessageReceived += (sender, args) =>
                {
                    var reader = new StreamReader(args.GetDataStream().AsStreamForRead());
                    {
                        string response = reader.ReadToEndAsync().Result;
                        OnMessageReceived(new MessageReceivedArgs(response));
                    }
                };

                socket.BindServiceNameAsync("", profile.NetworkAdapter);

                var remoteHost = new global::Windows.Networking.HostName(SSDP.Address);
                var reqBuff = Encoding.UTF8.GetBytes(data);

                var stream = await socket.GetOutputStreamAsync(remoteHost, SSDP.Port.ToString());
                await stream.WriteAsync(reqBuff.AsBuffer());

                if (socketJoined) return 0;

                socket.JoinMulticastGroup(remoteHost);
                socketJoined = !socketJoined;

                return 0;
            }
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
    }
}