﻿using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using ConnectSdk.Windows.Service;
using UpdateControls.XAML;

namespace ConnectSdk.Demo.Demo
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var target = parameter as string;
            var services = ForView.Unwrap<ObservableCollection<NetcastTvService>>(value);
            var isNetcast = services != null;

            switch (target)
            {
                case "PairingKeyLabel":
                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
                case "PairingKeyTextBox":
                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
                case "ConnectButton":
                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
                case "ConnectWebOsButton":
                    return !isNetcast ? Visibility.Visible : Visibility.Collapsed;
                case "ShowKeyButton":
                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
                    
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}