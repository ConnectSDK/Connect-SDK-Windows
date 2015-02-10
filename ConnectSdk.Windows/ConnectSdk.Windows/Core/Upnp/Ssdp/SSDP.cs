namespace ConnectSdk.Windows.Core.Upnp.Ssdp
{
    class SSDP
    {
        public static string Newline = "\r\n";

        public static string Address = "239.255.255.250";
        public static int Port = 1900;
        public static int SourcePort = 1901;

        public static string St = "ST";
        public static string Location = "LOCATION";
        public static string Nt = "NT";
        public static string Nts = "NTS";
        public static string Urn = "URN";
        public static string Usn = "USN";
        public static string ApplicationUrl = "Application-URL";

        /* Definitions of start line */
        public static string SlNotify = "NOTIFY * HTTP/1.1";
        public static string SlMsearch = "M-SEARCH * HTTP/1.1";
        public static string SlOk = "HTTP/1.1 200 OK";

        /* Definitions of search targets */
        public static string StSsap = St + ": urn:lge-com:service:webos-second-screen:1";
        public static string StDial = St + ": urn:dial-multiscreen-org:service:dial:1";
        public static string DeviceMediaServer1 = "urn:schemas-upnp-org:device:MediaServer:1";


        /* Definitions of notification sub type */
        public static string NtsAlive = "ssdp:alive";
        public static string NtsByebye = "ssdp:byebye";
        public static string NtsUpdate = "ssdp:update";

        public static ParsedDatagram ConvertDatagram(string datagramPacket)
        {
            return new ParsedDatagram(datagramPacket);
        }
    }
}
