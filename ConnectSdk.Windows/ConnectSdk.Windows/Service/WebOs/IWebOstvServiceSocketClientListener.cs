using Windows.Data.Json;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.WebOs
{
    public interface IWebOstvServiceSocketClientListener
    {
        void OnConnect();
        void OnCloseWithError(ServiceCommandError error);
        void OnFailWithError(ServiceCommandError error);

        void OnBeforeRegister();
        void OnRegistrationFailed(ServiceCommandError error);
        bool OnReceiveMessage(JsonObject message);
    }
}