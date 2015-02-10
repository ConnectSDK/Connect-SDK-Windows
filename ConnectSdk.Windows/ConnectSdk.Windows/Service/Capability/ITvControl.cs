using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface ITvControl
    {
        ITvControl GetTvControl();
        CapabilityPriorityLevel GetTvControlCapabilityLevel();

        void ChannelUp(ResponseListener listener);
        void ChannelDown(ResponseListener listener);

        void SetChannel(ChannelInfo channelNumber, ResponseListener listener);

        void GetCurrentChannel(ResponseListener listener);
        IServiceSubscription SubscribeCurrentChannel(ResponseListener listener);

        void GetChannelList(ResponseListener listener);

        void GetProgramInfo(ResponseListener listener);
        IServiceSubscription SubscribeProgramInfo(ResponseListener listener);

        void GetProgramList(ResponseListener listener);
        IServiceSubscription SubscribeProgramList(ResponseListener listener);

        void Get3DEnabled(ResponseListener listener);
        void Set3DEnabled(bool enabled, ResponseListener listener);
        IServiceSubscription Subscribe3DEnabled(ResponseListener listener);
    }
}