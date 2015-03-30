using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceCommandProcessor<T> where T: ResponseListener<object>
    {
        void Unsubscribe(UrlServiceSubscription<T> subscription);
        void Unsubscribe(IServiceSubscription<T> subscription);
        void SendCommand(ServiceCommand<T> command);
    }
}