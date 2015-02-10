using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using MyRemote.ConnectSDK.Core;
using MyRemote.ConnectSDK.Service.Capability.Listeners;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface IToastControl
    {
        IToastControl GetToastControl();
        CapabilityPriorityLevel GetToastControlCapabilityLevel();

        void ShowToast(string message, ResponseListener listener);
        void ShowToast(string message, string iconData, string iconExtension, ResponseListener listener);

        void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, ResponseListener listener);

        void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, string iconData,string iconExtension, ResponseListener listener);

        void ShowClickableToastForUrl(string message, string url, ResponseListener listener);

        void ShowClickableToastForUrl(string message, string url, string iconData, string iconExtension,ResponseListener listener);
    }
}