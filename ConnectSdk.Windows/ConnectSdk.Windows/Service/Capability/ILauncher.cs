using System;
using ConnectSdk.Windows.Core;
using MyRemote.ConnectSDK.Core;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface ILauncher
    {
        ILauncher GetLauncher();
        CapabilityPriorityLevel GetLauncherCapabilityLevel();

        void LaunchAppWithInfo(AppInfo appInfo, ResponseListener listener);
        void LaunchAppWithInfo(AppInfo appInfo, Object ps, ResponseListener listener);
        void LaunchApp(string appId, ResponseListener listener);

        void CloseApp(LaunchSession launchSession, ResponseListener listener);

        void GetAppList(ResponseListener listener);

        void GetRunningApp(ResponseListener listener);
        IServiceSubscription SubscribeRunningApp(ResponseListener listener);

        void GetAppState(LaunchSession launchSession, ResponseListener listener);

        IServiceSubscription SubscribeAppState(LaunchSession launchSession,
            ResponseListener listener);

        void LaunchBrowser(string url, ResponseListener listener);
        void LaunchYouTube(string contentId, ResponseListener listener);
        void LaunchNetflix(string contentId, ResponseListener listener);
        void LaunchHulu(string contentId, ResponseListener listener);
        void LaunchAppStore(string appId, ResponseListener listener);
    }
}