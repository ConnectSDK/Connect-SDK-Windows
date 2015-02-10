using MyRemote.ConnectSDK.Service.Capability.Listeners;

namespace MyRemote.ConnectSDK.Service.Capability
{
    public interface IPowerControl
    {
        IPowerControl GetPowerControl();
        CapabilityPriorityLevel GetPowerControlCapabilityLevel();

        void PowerOff(ResponseListener listener);
        void PowerOn(ResponseListener listener);
    }
}