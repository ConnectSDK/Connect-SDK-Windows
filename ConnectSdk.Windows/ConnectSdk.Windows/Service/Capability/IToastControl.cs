using Windows.Data.Json;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IToastControl
    {
        IToastControl GetToastControl();
        CapabilityPriorityLevel GetToastControlCapabilityLevel();

        void ShowToast(string message, ResponseListener<object> listener);
        void ShowToast(string message, string iconData, string iconExtension, ResponseListener<object> listener);

        void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, ResponseListener<object> listener);

        void ShowClickableToastForApp(string message, AppInfo appInfo, JsonObject ps, string iconData, string iconExtension, ResponseListener<object> listener);

        void ShowClickableToastForUrl(string message, string url, ResponseListener<object> listener);

        void ShowClickableToastForUrl(string message, string url, string iconData, string iconExtension, ResponseListener<object> listener);
    }
}