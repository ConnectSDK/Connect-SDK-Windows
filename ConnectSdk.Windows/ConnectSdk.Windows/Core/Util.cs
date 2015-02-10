using System;
using System.IO;
using System.Linq;
using Windows.Networking.Connectivity;

namespace ConnectSdk.Windows.Core
{
    public class Util
    {
        /// <summary>
        /// Converts a numeric ip to a list of bytes
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static byte[] ConvertIpAddress(int ip)
        {
            return new[] {
                (byte) (ip & 0xFF), 
                (byte) ((ip >> 8) & 0xFF), 
                (byte) ((ip >> 16) & 0xFF), 
                (byte) ((ip >> 24) & 0xFF)};
        }

        public static long GetTime()
        {
            //Todo: fix this. I don't know what they want
            return DateTime.Now.Millisecond;
        }

        //public static void PostSuccess(ResponseListener listener, object obj) 
        //{
        //    if (listener == null)
        //        return;
        //    listener.OnSuccess(obj);
        //}

        //public static void PostError(ResponseListener listener, ServiceCommandError error)
        //{
        //    if (listener == null)
        //        return;
        //    listener.OnError(error);
        //}

        /// <summary>
        /// checks if there is a wireless network connection
        /// </summary>
        public static bool IsWirelessAvailable()
        {
            var hnames = NetworkInformation.GetHostNames();
            return hnames.Where(hostName => hostName.IPInformation != null).Any(hostName => hostName.IPInformation.NetworkAdapter.IanaInterfaceType == 71);
        }

        /// <summary>
        /// Gets the wireless ip address
        /// </summary>
        /// <returns></returns>
        public static string GetLocalWirelessIp()
        {
            //TODO: check here for wireless to be active and throw exception when not
            var hnames = NetworkInformation.GetHostNames();
            return (from hostName in hnames 
                    where hostName.IPInformation != null 
                    where hostName.IPInformation.NetworkAdapter.IanaInterfaceType == 71 
                    select hostName.CanonicalName).FirstOrDefault();
        }

        /// <summary>
        /// Creates a stream from a string
        /// </summary>
        /// <param name="source">The source string</param>
        /// <returns>A stream used for reading from the source string</returns>
        public static Stream GenerateStreamFromstring(string source)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(source);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
	}
}