namespace ConnectSdk.Windows.Service.Config
{
    public interface IServiceConfigListener
    {
        void OnServiceConfigUpdate(ServiceConfig serviceConfig);
    }
}