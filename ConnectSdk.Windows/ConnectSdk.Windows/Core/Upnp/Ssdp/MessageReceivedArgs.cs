using System;

namespace ConnectSdk.Windows.Core.Upnp.Ssdp
{
    public class MessageReceivedArgs : EventArgs
    {
        public string Message { get; set; }

        public MessageReceivedArgs(string message)
        {
            Message = message;
        }
    }
}