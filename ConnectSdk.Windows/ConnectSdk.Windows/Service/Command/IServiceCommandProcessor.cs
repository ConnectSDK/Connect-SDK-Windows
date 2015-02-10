using MyRemote.ConnectSDK.Service;

namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceCommandProcessor
    {
        void Unsubscribe(UrlServiceSubscription subscription);
        void SendCommand(ServiceCommand command);
    }
}