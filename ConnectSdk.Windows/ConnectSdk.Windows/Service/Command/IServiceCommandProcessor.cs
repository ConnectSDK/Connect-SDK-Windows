namespace MyRemote.ConnectSDK.Service.Command
{
    public interface IServiceCommandProcessor
    {
        void Unsubscribe(UrlServiceSubscription subscription);
        void SendCommand(ServiceCommand command);
    }
}