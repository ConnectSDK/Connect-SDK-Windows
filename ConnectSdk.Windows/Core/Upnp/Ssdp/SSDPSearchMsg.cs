#region Copyright Notice
/*
 * ConnectSdk.Windows
 * SsdpSearchMsg.cs
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
using System.Text; 

namespace ConnectSdk.Windows.Core.Upnp.Ssdp
{
    public class SsdpSearchMsg
    {
        static readonly string Host = "HOST: " + SSDP.Address + ":" + SSDP.Port;
        private const string Man = "MAN: \"ssdp:discover\"";
        private const string Udap = "USER-AGENT: UDAP/2.0";
        private int mMx = 5;    /* seconds to delay response */

        public int MMx
        {
            get { return mMx; }
            set { mMx = value; }
        }

        public string MSt { get; set; }


        public SsdpSearchMsg(string st)
        {
            MSt = st;
        }

        public override string ToString()
        {
            var content = new StringBuilder();

            content.Append(SSDP.SlMsearch).Append(SSDP.Newline);
            content.Append(Host).Append(SSDP.Newline);
            content.Append(Man).Append(SSDP.Newline);
            content.Append(SSDP.St + ": " + MSt).Append(SSDP.Newline);
            content.Append("MX: " + MMx).Append(SSDP.Newline);
            if (MSt.Contains("udap"))
            {
                content.Append(Udap).Append(SSDP.Newline);
            }
            content.Append(SSDP.Newline);

            return content.ToString();
        }
    }
}