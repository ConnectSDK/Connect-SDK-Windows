namespace MyRemote.ConnectSDK.Service.Config
{
    public interface IServiceConfigListener
    {
        void OnServiceConfigUpdate(ServiceConfig serviceConfig);
    }
}