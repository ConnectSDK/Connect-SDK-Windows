using System.Collections.Generic;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class UrlServiceSubscription<T> : ServiceCommand<T>, IServiceSubscription<T>
    {
        private readonly List<ResponseListener<T>> listeners = new List<ResponseListener<T>>();

        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload,
            ResponseListener<T> listener) :
                base(service, uri, payload, listener)
        {
        }


        public UrlServiceSubscription(DeviceService service, string uri, JsonObject payload, bool isWebOs,
            ResponseListener<T> listener) :
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
            //todo: check this cast
            Service.SendCommand(this);

        }

        public void Unsubscribe()
        {
            //todo fix this
            Service.Unsubscribe(this);
        }

        public List<ResponseListener<T>> GetListeners()
        {
            throw new System.NotImplementedException();
        }

        public ResponseListener<T> AddListener(ResponseListener<T> listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public void RemoveListener(ResponseListener<T> listener)
        {
            listeners.Remove(listener);
        }

        public void RemoveListeners()
        {
            listeners.Clear();
        }


    }
}