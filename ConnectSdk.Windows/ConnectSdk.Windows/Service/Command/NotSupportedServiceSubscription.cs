using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public class NotSupportedServiceSubscription : IServiceSubscription
    {
        private readonly List<ResponseListener> listeners = new List<ResponseListener>();


        public void Unsubscribe()
        {
        }


        public ResponseListener AddListener(ResponseListener listener)
        {
            listeners.Add(listener);

            return listener;
        }

        public List<ResponseListener> GetListeners()
        {
            return listeners;
        }

        public void RemoveListener(ResponseListener listener)
        {
            listeners.Remove(listener);
        }
    }
}