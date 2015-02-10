using System.Text;

namespace ConnectSdk.Windows.Core.Upnp.Ssdp
{
    public class SSDPSearchMsg
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


        public SSDPSearchMsg(string st)
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