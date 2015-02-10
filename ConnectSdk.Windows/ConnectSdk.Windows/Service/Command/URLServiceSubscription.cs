using System.Collections.Generic;
using Windows.Data.Json;


namespace MyRemote.ConnectSDK.Service.Command
{
    public class UrlServiceSubscription : ServiceCommand, IServiceSubscription // where T : ResponseListener
    {
        private readonly List<Capability.Listeners.ResponseListener> listeners = new List<Capability.Listeners.ResponseListener>();

        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload,
            Capability.Listeners.ResponseListener listener) :
                base(service, uri, payload, listener)
        {
        }


        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload, bool isWebOs,
            Capability.Listeners.ResponseListener listener) :
                base(service, uri, payload, isWebOs, listener)
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
            if (!(HttpMethod.Equals(TYPE_GET)
                  || HttpMethod.Equals(TYPE_POST)))
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

        public Capability.Listeners.ResponseListener AddListener(Capability.Listeners.ResponseListener listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public void RemoveListener(Capability.Listeners.ResponseListener listener)
        {
            listeners.Remove(listener);
        }

        public void RemoveListeners()
        {
            listeners.Clear();
        }

        public List<Capability.Listeners.ResponseListener> GetListeners()
        {
            return listeners;
        }
    }
}