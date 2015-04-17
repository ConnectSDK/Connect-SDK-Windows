using System;
using Windows.Data.Json;
using ConnectSdk.Windows.Discovery;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.WebOs
{
    public class WebOstvServiceSocketClientListener : IWebOstvServiceSocketClientListener
    {
        private readonly IDeviceServiceListener listener;
        private readonly DeviceService service;

        public WebOstvServiceSocketClientListener(DeviceService deviceService, IDeviceServiceListener deviceListener)
        {
            listener = deviceListener;
            service = deviceService;
        }

        public void OnConnect()
        {
            listener.OnConnectionSuccess(service);
        }

        public void OnCloseWithError(ServiceCommandError error)
        {
            service.Disconnect();
            if (listener != null)
                listener.OnConnectionFailure(service, new Exception(error.GetCode().ToString()));
        }

        public void OnFailWithError(ServiceCommandError error)
        {
            if (listener != null)
                listener.OnConnectionFailure(service, new Exception(error.GetCode().ToString()));
        }

        public void OnBeforeRegister(PairingType pairingType)
        {
            if (DiscoveryManager.GetInstance().PairingLevel == DiscoveryManager.PairingLevelEnum.On)
            {
                if (listener != null)
                    listener.OnPairingRequired(service, pairingType, null);
            }
        }

        public void OnRegistrationFailed(ServiceCommandError error)
        {
            service.Disconnect();
            if (listener != null)
                listener.OnConnectionFailure(service, new Exception(error.GetCode().ToString()));
        }

        public bool OnReceiveMessage(JsonObject message)
        {
            return true;
        }
    }
}