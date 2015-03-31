using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceSubscription<T>
    {
        void Unsubscribe();
        ResponseListener<T> AddListener(ResponseListener<T> listener);
        void RemoveListener(ResponseListener<T> listener);
        List<ResponseListener<T>> GetListeners();
    }
}