#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ParsedDatagram.cs
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