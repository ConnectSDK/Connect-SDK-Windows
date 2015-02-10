using System.Collections.Generic;
using MyRemote.ConnectSDK.Service.Capability.Listeners;

namespace MyRemote.ConnectSDK.Service.Command
{
    /// <summary>
    /// Internal implementation of ServiceSubscription for URL-based commands
    /// </summary>
    public interface IServiceSubscription
    {
        void Unsubscribe();
        ResponseListener AddListener(ResponseListener listener);
        void RemoveListener(ResponseListener listener);
        List<ResponseListener> GetListeners();
    }
}
