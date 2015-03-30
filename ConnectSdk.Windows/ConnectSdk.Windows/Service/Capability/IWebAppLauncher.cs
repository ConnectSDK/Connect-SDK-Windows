using Windows.Data.Json;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IWebAppLauncher<T> where T : ResponseListener<T>
    {
        IWebAppLauncher<T> GetWebAppLauncher();
        CapabilityPriorityLevel GetWebAppLauncherCapabilityLevel();

        void LaunchWebApp(string webAppId, ResponseListener<LaunchSession<T>> listener);
        void LaunchWebApp(string webAppId, bool relaunchIfRunning, ResponseListener<LaunchSession<T>> listener);
        void LaunchWebApp(string webAppId, JsonObject ps, ResponseListener<LaunchSession<T>> listener);
        void LaunchWebApp(string webAppId, JsonObject ps, bool relaunchIfRunning, ResponseListener<LaunchSession<T>> listener);

        void JoinWebApp(LaunchSession<T> webAppLaunchSession, ResponseListener<LaunchSession<T>> listener);
        void JoinWebApp(string webAppId, ResponseListener<LaunchSession<T>> listener);
        void CloseWebApp(LaunchSession<T> launchSession, ResponseListener<object> listener);

        void PinWebApp(string webAppId, ResponseListener<object> listener);
        void UnPinWebApp(string webAppId, ResponseListener<object> listener);
        void IsWebAppPinned(string webAppId, ResponseListener<bool> listener);
        IServiceSubscription<T> SubscribeIsWebAppPinned(string webAppId, ResponseListener<bool> listener);

    }
}