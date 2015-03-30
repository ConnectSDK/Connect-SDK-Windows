using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface ITextInputControl
    {
        ITextInputControl GetTextInputControl();
        CapabilityPriorityLevel GetTextInputControlCapabilityLevel();

        IServiceSubscription<TextInputStatusInfo> SubscribeTextInputStatus(ResponseListener<TextInputStatusInfo> listener);

        void SendText(string input);
        void SendEnter();
        void SendDelete();
    }
}