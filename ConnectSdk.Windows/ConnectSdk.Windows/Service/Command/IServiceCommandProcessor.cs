namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceCommandProcessor
    {
        void Unsubscribe(UrlServiceSubscription subscription);
        void Unsubscribe(IServiceSubscription subscription);
        void SendCommand(ServiceCommand command);
    }
}