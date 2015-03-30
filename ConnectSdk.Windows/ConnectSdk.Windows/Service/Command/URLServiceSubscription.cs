using System.Collections.Generic;
using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class UrlServiceSubscription<T> : ServiceCommand<T>, IServiceSubscription<T> where T : ResponseListener<object>
    {
        private readonly List<T> listeners = new List<T>();

        public UrlServiceSubscription(IServiceCommandProcessor<T> service, string uri, JsonObject payload, ResponseListener<object> listener) :
                base(service, uri, payload, listener)
        {
        }


        public UrlServiceSubscription(IServiceCommandProcessor<T> service, string uri, JsonObject payload, bool isWebOs, ResponseListener<object> listener) :
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

        public List<T> GetListeners()
        {
            throw new System.NotImplementedException();
        }

        public T AddListener(T listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public void RemoveListener(T listener)
        {
            listeners.Remove(listener);
        }

        public void RemoveListeners()
        {
            listeners.Clear();
        }


    }
}