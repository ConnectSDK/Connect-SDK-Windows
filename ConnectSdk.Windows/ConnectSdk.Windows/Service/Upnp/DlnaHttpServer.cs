using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
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
            this.listener.Dispose();
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

            using (var output = socket.OutputStream)
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
                        dr.DetachStream();
                        dr.Dispose();
                    }
                }
            }

            if (body == null) return;
            
            //todo: add processing here
        }

        private async Task WriteResponseAsync(string path, IOutputStream os)
        {
            using (Stream resp = os.AsStreamForWrite())
            {
                bool exists = true;
                try
                {
                    // Look in the Data subdirectory of the app package
                    string filePath = "Data" + path.Replace('/', '\\');
                    //using (Stream fs = await LocalFolder.OpenStreamForReadAsync(filePath))
                    //{
                    //    string header = String.Format("HTTP/1.1 200 OK\r\n" +
                    //                                  "Content-Length: {0}\r\n" +
                    //                                  "Connection: close\r\n\r\n",
                    //        fs.Length);
                    //    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    //    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    //    await fs.CopyToAsync(resp);
                    //}
                }
                catch (FileNotFoundException)
                {
                    exists = false;
                }

                if (!exists)
                {
                    byte[] headerArray = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 404 Not Found\r\n" +
                        "Content-Length:0\r\n" +
                        "Connection: close\r\n\r\n");
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                }

                await resp.FlushAsync();
            }
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(Port.ToString());

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
