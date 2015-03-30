using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class NotSupportedServiceSubscription<T> : IServiceSubscription<T> where T : ResponseListener<object>
    {
        private readonly List<T> listeners = new List<T>();


        public void Unsubscribe()
        {
        }


        public T AddListener(T listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public List<T> GetListeners()
        {
            return listeners;
        }

        public void RemoveListener(T listener)
        {
            listeners.Remove(listener);
        }
    }
}