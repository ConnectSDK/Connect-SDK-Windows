using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
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