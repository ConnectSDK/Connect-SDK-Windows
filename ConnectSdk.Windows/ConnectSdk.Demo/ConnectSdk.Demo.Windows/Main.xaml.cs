using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using ConnectSdk.Demo.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237
using ConnectSdk.Demo.Demo;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;
using ConnectSdk.Windows.Service.Config;
using ConnectSdk.Windows.Service.Sessions;
using UpdateControls.Collections;

namespace ConnectSdk.Demo
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Main : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private Model model = null;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public Main()
        {
            model = App.ApplicationModel;

            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

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
                var scrollPad = (arg.OriginalSource as TextBlock).Parent as Border;
                var pos = arg.GetPosition(scrollPad);
                if (scrollPad != null && (pos.X > 0 && pos.Y > 0 && pos.X < scrollPad.ActualWidth && pos.Y < scrollPad.ActualWidth))
                {
                    model.Tap();
                }
            }
            else
            {
                var arg = e as DoubleTappedRoutedEventArgs;
                // we filled the border with a textblock and we will get this first
                var scrollPad = (arg.OriginalSource as TextBlock).Parent as Border;
                var pos = arg.GetPosition(scrollPad);
                if (scrollPad != null && (pos.X > 0 && pos.Y > 0 && pos.X < scrollPad.ActualWidth && pos.Y < scrollPad.ActualWidth))
                {
                    model.Tap();
                }
            }
          
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);

            var netCastService = (NetcastTvService)model.SelectedDevice.GetServiceByName(NetcastTvService.Id);
            if (netCastService != null)
            {
                var appListResponseListener = new ResponseListener();

                model.IpAddress = netCastService.ServiceDescription.IpAddress;
                model.Port = netCastService.ServiceDescription.Port.ToString();

                appListResponseListener.Success += (sender, o) =>
                {
                    var apps = ((o as LoadEventArgs).Load as ServiceCommandError).GetPayload() as List<AppInfo>;
                    for (int i = 0; i < apps.Count; i++)
                    {
                        apps[i].SetUrl(model.IpAddress, model.Port);
                    }
                    model.Apps = new IndependentList<AppInfo>(apps);
                };
                appListResponseListener.Error += (sender, o) =>
                {

                };

                //netCastService.getChannelList(responseListener);
                netCastService.GetAppList(appListResponseListener);


                var channelListResponseListener = new ResponseListener();
                channelListResponseListener.Success += (sender, o) =>
                {
                    var channels = ((o as LoadEventArgs).Load as ServiceCommandError).GetPayload() as List<ChannelInfo>;
                    model.Channels = new IndependentList<ChannelInfo>(channels);
                };
                channelListResponseListener.Error += (sender, o) =>
                {

                };
                netCastService.GetChannelList(channelListResponseListener);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void TextBoxToSend_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);
            if (binding != null) binding.UpdateSource();
        }


        private void ButtonClear_OnClick(object sender, RoutedEventArgs e)
        {
            model.TextInput = "";
            // ugly code but since we use the control in a datatemplate we have to find it as opposed to refference it
            var textBoxToSend = ((e.OriginalSource as Button).Parent as StackPanel).Children[0] as TextBox;
;
            if (textBoxToSend == null) return;
            
            var binding = textBoxToSend.GetBindingExpression(TextBox.TextProperty);
            if (binding != null) binding.UpdateSource();
            textBoxToSend.Text = "";
        }

        private void PrintButton_OnClick(object sender, RoutedEventArgs e)
        {
            CallCaptureImage(e);
        }

        public void CallCaptureImage(RoutedEventArgs eva)
        {
            try
            {
                var img = ((eva.OriginalSource as Button).Parent as StackPanel).Children[1] as Image;
                var serviceDescription = model.SelectedDevice.GetServiceByName("Netcast TV").ServiceDescription;
                var baseurl = string.Format("http://{0}:{1}", serviceDescription.IpAddress, serviceDescription.Port);
                var url = baseurl + "/udap/api/data?target=screen_image&s=" + DateTime.Now.Ticks;

                img.Source = new BitmapImage(new Uri(url));
            }
            catch (Exception e)
            {
                
            }
        }

        private void AppImage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var appinfo = (e.OriginalSource as Image).DataContext as AppInfo;
            if (appinfo != null)
            {
                var netCastService = (NetcastTvService)model.SelectedDevice.GetServiceByName(NetcastTvService.Id);
                if (netCastService != null)
                {
                    ResponseListener listener = new ResponseListener();
                    listener.Error += (o, error) =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    };
                    netCastService.LaunchAppWithInfo(appinfo, listener);
                }
            }
        }

        private void ChannelListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var channelInfo = e.AddedItems[0] as ChannelInfo;
            if (channelInfo != null)
            {
                var netCastService = (NetcastTvService)model.SelectedDevice.GetServiceByName(NetcastTvService.Id);
                if (netCastService != null)
                {
                    ResponseListener listener = new ResponseListener();
                    listener.Error += (o, error) =>
                    {
                        var msg =
                            new MessageDialog(
                                "Something went wrong; The application could not be started. Press 'Close' to continue");
                        msg.ShowAsync();
                    };
                    netCastService.SetChannel(channelInfo, listener);
                }
            }
        }

        WebOsWebAppSession launchSession = null;
        private void OpenWebApp_Click(object sender, RoutedEventArgs e)
        {
            //var webappname = "BareMoon 2";
            var webappname = "YouTube";
            
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);
            ResponseListener listener = new ResponseListener();
            listener.Error += (o, error) =>
            {
                var msg =
                    new MessageDialog(
                        "Something went wrong; The application could not be started. Press 'Close' to continue");
                msg.ShowAsync();
            };

            listener.Success += (o, param) =>
            {
                var v = param as LoadEventArgs;
                if (v != null)
                    try
                    {
                        launchSession = v.Load.GetPayload() as WebOsWebAppSession;
                        
                        
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }
                    
            };

            webostvService.LaunchWebApp(webappname, listener);

            //webostvService.

            //webostvService.CloseWebApp();
        }

        private void CloserWebApp_Click(object sender, RoutedEventArgs e)
        {
            var webostvService = (WebOstvService)model.SelectedDevice.GetServiceByName(WebOstvService.Id);
            ResponseListener listener = new ResponseListener();
            listener.Error += (o, error) =>
            {
                var msg =
                    new MessageDialog(
                        "Something went wrong; The application could not be stopped. Press 'Close' to continue");
                msg.ShowAsync();
            };

            listener.Success += (o, param) =>
            {
                //var v = param as LoadEventArgs;
                //if (v != null)
                //    launchSession = v.Load.GetPayload() as LaunchSession;
            };

            webostvService.CloseWebApp(launchSession.LaunchSession, listener);
        }
    }
}
