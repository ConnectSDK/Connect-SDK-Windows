namespace ConnectSdk.Windows.Service.Command
{
    public interface IServiceCommandProcessor<T>
    {
        void Unsubscribe(UrlServiceSubscription<T> subscription);
        void Unsubscribe(IServiceSubscription<T> subscription);
        void SendCommand(ServiceCommand<T> command);
    }
}