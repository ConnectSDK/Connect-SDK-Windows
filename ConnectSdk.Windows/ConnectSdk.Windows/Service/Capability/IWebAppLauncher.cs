using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IWebAppLauncher
    {
        IWebAppLauncher GetWebAppLauncher();
        CapabilityPriorityLevel GetWebAppLauncherCapabilityLevel();

        void LaunchWebApp(string webAppId, ResponseListener<LaunchSession> listener);
        void LaunchWebApp(string webAppId, bool relaunchIfRunning, ResponseListener<LaunchSession> listener);
        void LaunchWebApp(string webAppId, JsonObject ps, ResponseListener<LaunchSession> listener);
        void LaunchWebApp(string webAppId, JsonObject ps, bool relaunchIfRunning, ResponseListener<LaunchSession> listener);

        void JoinWebApp(LaunchSession webAppLaunchSession, ResponseListener<LaunchSession> listener);
        void JoinWebApp(string webAppId, ResponseListener<LaunchSession> listener);
        void CloseWebApp(LaunchSession launchSession, ResponseListener<object> listener);

        void PinWebApp(string webAppId, ResponseListener<object> listener);
        void UnPinWebApp(string webAppId, ResponseListener<object> listener);
        void IsWebAppPinned(string webAppId, ResponseListener<bool> listener);
        IServiceSubscription SubscribeIsWebAppPinned(string webAppId, ResponseListener<bool> listener);

    }
}