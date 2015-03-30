using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IPowerControl
    {
        IPowerControl GetPowerControl();
        CapabilityPriorityLevel GetPowerControlCapabilityLevel();

        void PowerOff(ResponseListener<object> listener);
        void PowerOn(ResponseListener<object> listener);
    }
}