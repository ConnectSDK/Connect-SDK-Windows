using System;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Command;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface ITextInputControl
    {
        ITextInputControl GetTextInputControl();
        CapabilityPriorityLevel GetTextInputControlCapabilityLevel();

        IServiceSubscription SubscribeTextInputStatus(ResponseListener listener);

        void SendText(string input);
        void SendEnter();
        void SendDelete();
    }
}