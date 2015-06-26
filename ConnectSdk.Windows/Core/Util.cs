#region Copyright Notice
/*
 * ConnectSdk.Windows
 * util.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
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
using System.Linq;
using Windows.Networking.Connectivity;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

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

        public static void PostSuccess(ResponseListener listener, object obj)
        {
            if (listener == null)
                return;
            listener.OnSuccess(obj);
        }

        public static void PostError(ResponseListener listener, ServiceCommandError error)
        {
            if (listener == null)
                return;
            listener.OnError(error);
        }

        /// <summary>
        /// checks if there is a wireless network connection
        /// </summary>
        public static bool IsWirelessAvailable()
        {
            var hnames = NetworkInformation.GetHostNames();
            // check also IanaInterfaceType = 6 because we might be in an emulator on a PC :)
            return hnames.Where(hostName => hostName.IPInformation != null).Any(hostName => (hostName.IPInformation.NetworkAdapter.IanaInterfaceType == 71 || hostName.IPInformation.NetworkAdapter.IanaInterfaceType == 6));
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

        public static string ParseData(string response, string key)
        {
            string startTag = "<" + key + ">";
            string endTag = "</" + key + ">";

            int start = response.IndexOf(startTag, StringComparison.Ordinal);
            int end = response.IndexOf(endTag, StringComparison.Ordinal);

            string data = response.Substring(start + startTag.Length, end - start - startTag.Length);

            return data;
        }

        public static long ConvertStrTimeFormatToLong(string strTime)
        {
            var tokens = strTime.Split(':');
            long time = 0;

            foreach (var token in tokens)
            {
                time *= 60;
                time += int.Parse(token);
            }

            return time;
        }

        public static string DecToHex(string dec)
        {
            return !string.IsNullOrEmpty(dec) ? DecToHex(long.Parse(dec)) : null;
        }

        public static string DecToHex(long dec)
        {
            return dec.ToString("X");
        }
    }
}