using Windows.Data.Json;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface IWebAppLauncher
    {
        IWebAppLauncher GetWebAppLauncher();
        CapabilityPriorityLevel GetWebAppLauncherCapabilityLevel();

        void LaunchWebApp(string webAppId, ResponseListener listener);
        void LaunchWebApp(string webAppId, bool relaunchIfRunning, ResponseListener listener);
        void LaunchWebApp(string webAppId, JsonObject ps, ResponseListener listener);
        void LaunchWebApp(string webAppId, JsonObject ps, bool relaunchIfRunning, ResponseListener listener);
        void JoinWebApp(LaunchSession webAppLaunchSession, ResponseListener listener);
        void JoinWebApp(string webAppId, ResponseListener listener);
        void CloseWebApp(LaunchSession launchSession, ResponseListener listener);
    }
}