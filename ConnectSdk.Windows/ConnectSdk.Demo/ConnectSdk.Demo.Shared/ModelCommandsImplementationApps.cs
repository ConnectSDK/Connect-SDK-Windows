using System;
using System.Collections.Generic;
using Windows.UI.Core;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;

namespace ConnectSdk.Demo.Demo
{
    public partial class Model
    {
        private LaunchSession runningAppSession;
        private AppInfo selectedApp;
        private List<AppInfo> appsList;
        private LaunchSession myAppSession;
        private LaunchSession appStoreSession;

        public List<AppInfo> AppsList
        {
            get { return appsList; }
            set { appsList = value; OnPropertyChanged(); }
        }

        public AppInfo SelectedApp
        {
            get { return selectedApp; }
            set
            {
                selectedApp = value; OnPropertyChanged();
                StartApp(selectedApp);
            }
        }

        private void SetControlApps()
        {
            if (selectedDevice != null)
            {
                GoogleCommand.Enabled = selectedDevice.HasCapability(Launcher.Browser);
                ShowToastCommand.Enabled = selectedDevice.HasCapability(ToastControl.ShowToast);
                NetflixCommand.Enabled = selectedDevice.HasCapability(Launcher.Netflix) || selectedDevice.HasCapability(Launcher.NetflixParams);
                YouTubeCommand.Enabled = selectedDevice.HasCapability(Launcher.YouTube);
                MyDialAppCommand.Enabled = selectedDevice.HasCapability("Launcher.Levak");
                AppStoreCommand.Enabled = selectedDevice.HasCapability(Launcher.AppStoreParams);

                if (selectedDevice.HasCapability(Launcher.RunningAppSubscribe))
                {
                    var runningAppListener = new ResponseListener
                        (
                        loadEventArg =>
                        {
                            var appInfo = LoadEventArgs.GetValue<AppInfo>(loadEventArg);

                            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                            {
                                selectedApp = appInfo;
                            });

                        },
                        serviceCommandError =>
                        {

                        }
                        );
                    launcher.SubscribeRunningApp(runningAppListener);
                }

                if (selectedDevice.HasCapability(Launcher.ApplicationList))
                {
                    var rappListListener = new ResponseListener
                        (
                        loadEventArg =>
                        {
                            var apps = LoadEventArgs.GetValue<List<AppInfo>>(loadEventArg);

                            App.MainDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                            {
                                AppsList = apps;
                            });
                        },
                        serviceCommandError =>
                        {

                        }
                        );
                    launcher.GetAppList(rappListListener);
                }

            }


        }

        private void GoogleCommandExecute(object obj)
        {
            if (runningAppSession != null)
            {
                runningAppSession.Close(null);
                runningAppSession = null;
            }
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var session = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    SetRunningAppInfo(session);
                },
                serviceCommandError =>
                {

                }
                );

            launcher.LaunchBrowser("http://connectsdk.com/", listener);
        }

        private void SetRunningAppInfo(LaunchSession session)
        {
            runningAppSession = session;
        }

        private void MyDialAppCommandExecute(object obj)
        {
            if (runningAppSession != null)
            {
                myAppSession.Close(null);
                myAppSession = null;
            }

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var session = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    myAppSession = session;
                },
                serviceCommandError =>
                {

                }
                );

            launcher.LaunchApp("Levak", listener);
        }

        private void ShowToastCommandExecute(object obj)
        {
            toastControl.ShowToast("Yeah, toast!", GetToastIconData(), "png", null);
        }

        private string GetToastIconData()
        {
            return null;
        }

        private void NetflixCommandExecute(object obj)
        {
            if (runningAppSession != null)
            {
                runningAppSession.Close(null);
                runningAppSession = null;
            }
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var session = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    SetRunningAppInfo(session);
                },
                serviceCommandError =>
                {

                }
                );

            launcher.LaunchNetflix("70217913", listener);
        }

        private void AppStoreCommandExecute(object obj)
        {
            if (appStoreSession != null)
            {
                appStoreSession.Close(null);
                appStoreSession = null;
            }
            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var session = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    appStoreSession = session;
                },
                serviceCommandError =>
                {

                }
                );

            String appId = null;

            if (selectedDevice.GetServiceByName("Netcast TV") != null)
                appId = "125071";
            else if (selectedDevice.GetServiceByName("webOS TV") != null)
                appId = "redbox";
            else if (selectedDevice.GetServiceByName("Roku") != null)
                appId = "13535";

            launcher.LaunchAppStore(appId, listener);
        }

        private void YouTubeCommandExecute(object obj)
        {
            if (runningAppSession != null)
            {
                runningAppSession.Close(null);
                runningAppSession = null;
            }

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var session = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    SetRunningAppInfo(session);
                },
                serviceCommandError =>
                {

                }
                );

            launcher.LaunchYouTube("eRsGyueVLvQ", listener);
        }

        private void StartApp(object arg)
        {
            var app = arg as AppInfo;

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var session = LoadEventArgs.GetValue<LaunchSession>(loadEventArg);
                    SetRunningAppInfo(session);
                },
                serviceCommandError =>
                {

                }
                );

            launcher.LaunchAppWithInfo(app, null, listener);
        }
    }
}
