using System.Collections.Generic;
using MyRemote.ConnectSDK.Service.Capability.Listeners;

namespace MyRemote.ConnectSDK.Service.Command
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