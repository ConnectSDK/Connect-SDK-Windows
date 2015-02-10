using System.Collections.Generic;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class UrlServiceSubscription : ServiceCommand, IServiceSubscription // where T : ResponseListener
    {
        private readonly List<ResponseListener> listeners = new List<ResponseListener>();

        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload,
            ResponseListener listener) :
                base(service, uri, payload, listener)
        {
        }


        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload, bool isWebOs,
            ResponseListener listener) :
                base(service, uri, payload, listener)
        {
            if (isWebOs)
                HttpMethod = "subscribe";
        }

        public new void Send()
        {
            Subscribe();
        }

        public void Subscribe()
        {
            if (!(HttpMethod.Equals(TypeGet)
                  || HttpMethod.Equals(TypePost)))
            {
                HttpMethod = "subscribe";
            }
            Service.SendCommand(this);

        }

        public void Unsubscribe()
        {
            //todo fix this
            //Service.Unsubscribe(this);
        }

        public ResponseListener AddListener(ResponseListener listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public void RemoveListener(ResponseListener listener)
        {
            listeners.Remove(listener);
        }

        public void RemoveListeners()
        {
            listeners.Clear();
        }

        public List<ResponseListener> GetListeners()
        {
            return listeners;
        }
    }
}