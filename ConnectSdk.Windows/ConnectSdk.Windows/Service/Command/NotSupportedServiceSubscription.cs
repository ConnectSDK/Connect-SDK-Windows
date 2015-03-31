using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class NotSupportedServiceSubscription<T> : IServiceSubscription<T>
    {
        private readonly List<ResponseListener<T>> listeners = new List<ResponseListener<T>>();


        public void Unsubscribe()
        {
        }


        public ResponseListener<T> AddListener(ResponseListener<T> listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public List<ResponseListener<T>> GetListeners()
        {
            return listeners;
        }

        public void RemoveListener(ResponseListener<T> listener)
        {
            listeners.Remove(listener);
        }
    }
}