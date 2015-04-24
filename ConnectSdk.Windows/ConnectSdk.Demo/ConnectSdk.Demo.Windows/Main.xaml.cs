using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
using ConnectSdk.Demo.Demo;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Sessions;
using WinRTXamlToolkit.Controls;

namespace ConnectSdk.Demo
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Main : Page
    {
        private readonly Model model;

        WebOsWebAppSession launchSession;
        private LaunchSession applaunchSession;

        public Main()
        {
            model = App.ApplicationModel;
            InitializeComponent();
            DataContext = model;
        }

        private void ScrollPadOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
        {
            model.Move(manipulationDeltaRoutedEventArgs.Delta.Translation.X, manipulationDeltaRoutedEventArgs.Delta.Translation.Y);
        }

        private void ScrollPadOnTapped(object sender, TappedRoutedEventArgs e)
        {
            ProcessTap(e);
        }

        /// <summary>
        /// this is used by the not touch enabled devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollPad_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ProcessTap(e);
        }

        private void ProcessTap(RoutedEventArgs e)
        {
            if (e is TappedRoutedEventArgs)
            {
                var arg = e as TappedRoutedEventArgs;
                // we filled the border with a textblock and we will get this first
                var textBlock = arg.OriginalSource as TextBlock;
                if (textBlock != null)
                {
                    var scrollPad = textBlock.Parent as Border;
                    var pos = arg.GetPosition(scrollPad);
                    if (scrollPad != null && (pos.X > 0 && pos.Y > 0 && pos.X < scrollPad.ActualWidth && pos.Y < scrollPad.ActualWidth))
                    {
                        model.Tap();
                    }
                }
            }
            else
            {
                var arg = e as DoubleTappedRoutedEventArgs;
                // we filled the border with a textblock and we will get this first
                if (arg != null)
                {
                    var textBlock = arg.OriginalSource as TextBlock;
                    if (textBlock != null)
                    {
                        var scrollPad = textBlock.Parent as Border;
                        var pos = arg.GetPosition(scrollPad);
                        if (scrollPad != null && (pos.X > 0 && pos.Y > 0 && pos.X < scrollPad.ActualWidth && pos.Y < scrollPad.ActualWidth))
                        {
                            model.Tap();
                        }
                    }
                }
            }

        }
        
        public void CallCaptureImage(RoutedEventArgs eva)
        {
            try
            {
                var button = eva.OriginalSource as Button;
                if (button != null)
                {
                    var stackPanel = button.Parent as StackPanel;
                    if (stackPanel != null)
                    {
                        var img = stackPanel.Children[1] as Image;
                        var serviceDescription = model.SelectedDevice.GetServiceByName("Netcast TV").ServiceDescription;
                        var baseurl = string.Format("http://{0}:{1}", serviceDescription.IpAddress, serviceDescription.Port);
                        var url = baseurl + "/udap/api/data?target=screen_image&s=" + DateTime.Now.Ticks;

                        if (img != null) img.Source = new BitmapImage(new Uri(url));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void OpenWebApp_Click(object sender, RoutedEventArgs e)
        {
            const string webappname = "SampleWebApp";

            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var listener = new ResponseListener
                (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null)
                    {
                        launchSession = v.Load.GetPayload() as WebOsWebAppSession;
                    }
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be started. Press 'Close' to continue");
                    msg.ShowAsync();
                }
                );

            webostvService.LaunchWebApp(webappname, listener);

        }

        private void CloserWebApp_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    //var v = param as LoadEventArgs;
                    //if (v != null)
                    //    launchSession = v.Load.GetPayload() as LaunchSession;
                },
                serviceCommandError =>
                {
                    var msg =
                        new MessageDialog(
                            "Something went wrong; The application could not be stopped. Press 'Close' to continue");
                    msg.ShowAsync();
                }
            );

            webostvService.CloseWebApp(launchSession.LaunchSession, responseListener);
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var wrapPanel = button.Parent as WrapPanel;
                if (wrapPanel != null)
                {
                    var webAppMessageBox = wrapPanel.Children[0] as TextBox;

                    var responseListener = new ResponseListener
                    (
                        loadEventArg =>
                        {

                        },
                        serviceCommandError =>
                        {
                            var msg =
                                new MessageDialog(
                                    "Something went wrong; Could not send message. Press 'Close' to continue");
                            msg.ShowAsync();
                        }
                    );

                    if (webAppMessageBox != null) launchSession.SendMessage(webAppMessageBox.Text, responseListener);
                }
            }
        }

        private void LaunchStore_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var responseListener = new ResponseListener
            (
            loadEventArg =>
            {

                var responseListener2 = new ResponseListener
                (
                    loadEventArg2 =>
                    {

                    },
                    serviceCommandError =>
                    {

                    }
                );
                webostvService.GetRunningApp(responseListener2);
            },
            serviceCommandError =>
            {

            }
            );
            webostvService.LaunchAppStore("", responseListener);
        }

        private void GetAppList_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var appListResponseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var loadEventArgs = loadEventArg as LoadEventArgs;
                    if (loadEventArgs != null)
                    {
                        var netCastService1 = (NetcastTvService)model.SelectedDevice.GetServiceByName(NetcastTvService.Id);
                        var webostvService1 = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);
                        var port = netCastService1 != null
                            ? netCastService1.ServiceDescription.Port.ToString()
                            : webostvService1.ServiceDescription.Port.ToString();

                        var apps = loadEventArgs.Load.GetPayload() as List<AppInfo>;
                        for (int i = 0; i < apps.Count; i++)
                        {
                            apps[i].SetUrl(model.SelectedDevice.IpAddress, port);
                            model.Apps.Add(apps[i]);
                        }
                        Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { model.OnPropertyChanged("Apps"); });
                    }
                },
                serviceCommandError =>
                {

                }
            );

            webostvService.GetAppList(appListResponseListener);
        }

        private void LauncApp_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {
                    var v = loadEventArg as LoadEventArgs;
                    if (v != null)
                    {
                        applaunchSession = v.Load.GetPayload() as LaunchSession;
                    }
                },
                serviceCommandError =>
                {

                }
            );

            webostvService.LaunchApp(model.Apps[1].Id, responseListener);
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {

                }
            );

            webostvService.CloseApp(applaunchSession, responseListener);
        }

        private void GetRunningApp_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            var responseListener = new ResponseListener
            (
                loadEventArg =>
                {

                },
                serviceCommandError =>
                {

                }
            );

            webostvService.GetRunningApp(responseListener);
        }


        private void MediaPlayerMedia_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            if (launchSession == null)
            {
                const string webappname = "MediaPlayer";

                var listener = new ResponseListener
                    (
                    loadEventArg =>
                    {
                        var v = loadEventArg as LoadEventArgs;
                        if (v != null)
                        {
                            launchSession = v.Load.GetPayload() as WebOsWebAppSession;

                            var listener2 = new ResponseListener
                                (
                                loadEventArg2 => webostvService.PlayMedia("http://connectsdk.com/files/8913/9657/0225/test_video.mp4", "video/mp4", "Sintel Trailer", "Blender Open Movie Project", "http://www.connectsdk.com/files/7313/9657/0225/test_video_icon.jpg", false, null),
                                serviceCommandError =>
                                {
                                });


                            if (launchSession != null) launchSession.Connect(listener2);
                        }
                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                    );

                webostvService.LaunchWebApp(webappname, listener);
            }
            else
            {
                webostvService.PlayMedia("http://www.connectsdk.com/files/8913/9657/0225/test_video.mp4", "video/mp4", "Sintel Trailer", "Blender Open Movie Project", "http://www.connectsdk.com/files/7313/9657/0225/test_video_icon.jpg", false, null);
            }


        }

        private void MediaPlayerImage_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);

            if (launchSession == null)
            {
                const string webappname = "MediaPlayer";
                var listener = new ResponseListener
                    (
                    loadEventArg =>
                    {
                        var v = loadEventArg as LoadEventArgs;
                        if (v != null)
                        {
                            launchSession = v.Load.GetPayload() as WebOsWebAppSession;

                            var listener2 = new ResponseListener
                                (
                                loadEventArg2 => webostvService.DisplayImage(
                                    "http://connectsdk.com/files/9613/9656/8539/test_image.jpg", "image/jpeg",
                                    "Sintel Character Design", "Blender Open Movie Project",
                                    "http://connectsdk.com/files/2013/9656/8845/test_image_icon.jpg", null),
                                serviceCommandError =>
                                {
                                });


                            if (launchSession != null) launchSession.Connect(listener2);
                        }
                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                    );

                webostvService.LaunchWebApp(webappname, listener);
            }
            else
            {
                webostvService.DisplayImage(
                    "http://www.connectsdk.com/files/9613/9656/8539/test_image.jpg", "image/jpeg",
                    "Sintel Character Design", "Blender Open Movie Project",
                    "http://www.connectsdk.com/files/2013/9656/8845/test_image_icon.jpg", null);
            }


        }

        private void AppListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var appinfo = e.AddedItems[0] as AppInfo;
            if (appinfo != null)
            {
                var dservice = model.SelectedDevice.GetServices()[0];

                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {

                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                );

                if (dservice is ILauncher)
                    (dservice as ILauncher).LaunchAppWithInfo(appinfo, responseListener);

            }
        }

        private void ChannelListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var channelInfo = e.AddedItems[0] as ChannelInfo;
            if (channelInfo != null)
            {
                var dservice = model.SelectedDevice.GetServices()[0];

                var responseListener = new ResponseListener
                (
                    loadEventArg =>
                    {

                    },
                    serviceCommandError =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    }
                );

                if (dservice is ITvControl)
                    (dservice as ITvControl).SetChannel(channelInfo, responseListener);
            }
        }

    }
}
