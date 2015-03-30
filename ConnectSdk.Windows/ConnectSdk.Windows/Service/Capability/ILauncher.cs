using System;
using System.Collections.Generic;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface ILauncher
    {
        ILauncher GetLauncher();
        CapabilityPriorityLevel GetLauncherCapabilityLevel();

        void LaunchAppWithInfo(AppInfo appInfo, ResponseListener<LaunchSession> listener);
        void LaunchAppWithInfo(AppInfo appInfo, Object ps, ResponseListener<LaunchSession> listener);
        void LaunchApp(string appId, ResponseListener<LaunchSession> listener);

        void CloseApp(LaunchSession launchSession, ResponseListener<object> listener);

        void GetAppList(ResponseListener<List<AppInfo>> listener);

        void GetRunningApp(ResponseListener<AppInfo> listener);
        IServiceSubscription<AppInfo> SubscribeRunningApp(ResponseListener<AppInfo> listener);

        void GetAppState(LaunchSession launchSession, ResponseListener<AppState> listener);
        IServiceSubscription<AppState> SubscribeAppState(LaunchSession launchSession, ResponseListener<AppState> listener);

        void LaunchBrowser(string url, ResponseListener<LaunchSession> listener);
        void LaunchYouTube(string contentId, ResponseListener<LaunchSession> listener);
        void LaunchNetflix(string contentId, ResponseListener<LaunchSession> listener);
        void LaunchHulu(string contentId, ResponseListener<LaunchSession> listener);
        void LaunchAppStore(string appId, ResponseListener<LaunchSession> listener);
    }
}