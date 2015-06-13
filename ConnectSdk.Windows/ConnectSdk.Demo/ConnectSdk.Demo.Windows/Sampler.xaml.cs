using Windows.System;
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

namespace ConnectSdk.Demo
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
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
            this.InitializeComponent();

            model = App.ApplicationModel;
            InitializeComponent();
            DataContext = model;

            model.SetControls();

        }

        private void RangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            
        }

        private void VolumeRangeBase_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.NewValue != model.Volume)
            {
                model.SetVolume(e.NewValue/100);
            }
        }

        private void InputTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void InputTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            model.SendText(e.Key.ToString());
            
        }
    }
}
