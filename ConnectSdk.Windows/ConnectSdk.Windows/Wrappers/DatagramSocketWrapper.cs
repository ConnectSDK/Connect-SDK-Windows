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