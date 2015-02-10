using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IExternalInputControl : IControl
    {
        IExternalInputControl GetExternalInput();
        CapabilityPriorityLevel GetExternalInputControlPriorityLevel();

        void LaunchInputPicker(ResponseListener listener);
        //void CloseInputPicker(LaunchSession launchSessionm, ResponseListener listener);

        //void GetExternalInputList(ExternalInputListListener listener);
        void SetExternalInput(ExternalInputInfo input, ResponseListener listener);
    }
}
