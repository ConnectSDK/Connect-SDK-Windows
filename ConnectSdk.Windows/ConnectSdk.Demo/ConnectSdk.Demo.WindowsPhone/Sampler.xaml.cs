using ConnectSdk.Demo.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using ConnectSdk.Demo.Demo;

namespace ConnectSdk.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Sampler : Page
    {
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Model model;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public Sampler()
        {
            InitializeComponent();

            model = App.ApplicationModel;
            InitializeComponent();
            DataContext = model;

            model.SetControls();

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += delegate
            {
                if (dispatcherTimer == null) return;

                model.GetVolume();
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

        private void VolumeRangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue != model.Volume)
            {
                model.SetVolume(e.NewValue / 100);
            }
        }

        private void InputTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            model.SendText(e.Key.ToString());

        }



    }
}
