//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Data;
//using ConnectSdk.Windows.Service;

//namespace ConnectSdk.Demo
//{
//    public class VisibilityConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, string language)
//        {
//            var target = parameter as string;
//            var services = value as List<DeviceService>;
//            var isNetcast = services.Any(x => x.ServiceName == "Netcast TV");

//            switch (target)
//            {
//                case "PairingKeyLabel":
//                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
//                case "PairingKeyTextBox":
//                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
//                case "ConnectButton":
//                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
//                case "ConnectWebOsButton":
//                    return !isNetcast ? Visibility.Visible : Visibility.Collapsed;
//                case "ShowKeyButton":
//                    return !isNetcast ? Visibility.Collapsed : Visibility.Visible;
                    
//            }
//            return Visibility.Collapsed;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, string language)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}