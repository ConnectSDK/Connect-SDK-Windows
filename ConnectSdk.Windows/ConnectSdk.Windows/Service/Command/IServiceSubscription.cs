using System.Collections.Generic;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceSubscription<T> 
    {
        void Unsubscribe();
        T AddListener(T listener);
        void RemoveListener(T listener);
        List<T> GetListeners();
    }
}