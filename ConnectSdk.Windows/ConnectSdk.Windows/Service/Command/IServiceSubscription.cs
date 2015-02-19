using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceSubscription
    {
        void Unsubscribe();
        ResponseListener AddListener(ResponseListener listener);
        void RemoveListener(ResponseListener listener);
        List<ResponseListener> GetListeners();
    }
}