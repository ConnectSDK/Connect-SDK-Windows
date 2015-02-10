using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectSdk.Windows.Core.Upnp.Ssdp
{
    public class ParsedDatagram
    {
        public string DataPacket;
        public Dictionary<string, string> Data = new Dictionary<string, string>();
        public string PacketType;

        public ParsedDatagram(string packet)
        {
            DataPacket = packet;

            var lines = packet.Split('\n').ToList();

            PacketType = lines[0];
            var linec = -1;

            while (linec < lines.Count - 1)
            {
                linec++;
                string line = lines[linec];
                int index = line.IndexOf(':');
                if (index == -1)
                {
                    continue;
                }
                Data.Add(line.Substring(0, index).ToUpperInvariant(), line.Substring(index + 1).Trim());
            }
        }
    }
}