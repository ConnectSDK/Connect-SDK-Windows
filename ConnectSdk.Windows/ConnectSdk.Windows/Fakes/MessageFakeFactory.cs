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
