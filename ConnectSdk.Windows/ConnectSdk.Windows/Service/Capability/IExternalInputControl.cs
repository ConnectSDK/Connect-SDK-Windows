using ConnectSdk.Windows.Core;
using MyRemote.ConnectSDK.Core;
using MyRemote.ConnectSDK.Service.Capability.Listeners;
using MyRemote.ConnectSDK.Service.Sessions;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface IExternalInputControl : IControl
    {
        IExternalInputControl GetExternalInput();
        CapabilityPriorityLevel GetExternalInputControlPriorityLevel();

        void LaunchInputPicker(ResponseListener listener);
        void CloseInputPicker(LaunchSession launchSessionm, ResponseListener listener);

        void GetExternalInputList(ExternalInputListListener listener);
        void SetExternalInput(ExternalInputInfo input, ResponseListener listener);
    }
}
