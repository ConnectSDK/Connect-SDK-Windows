using System.Collections.Generic;
using ConnectSdk.Windows.Core;
using ConnectSdk.Windows.Service.Capability.Listeners;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface ITvControl
    {
        ITvControl GetTvControl();
        CapabilityPriorityLevel GetTvControlCapabilityLevel();

        void ChannelUp(ResponseListener<object> listener);
        void ChannelDown(ResponseListener<object> listener);

        void SetChannel(ChannelInfo channelNumber, ResponseListener<object> listener);

        void GetCurrentChannel(ResponseListener<ChannelInfo> listener);
        IServiceSubscription<ChannelInfo> SubscribeCurrentChannel(ResponseListener<ChannelInfo> listener);

        void GetChannelList(ResponseListener<List<ChannelInfo>> listener);

        void GetProgramInfo(ResponseListener<ProgramInfo> listener);
        IServiceSubscription<ProgramInfo> SubscribeProgramInfo(ResponseListener<ProgramInfo> listener);

        void GetProgramList(ResponseListener<ProgramList> listener);
        IServiceSubscription<ProgramList> SubscribeProgramList(ResponseListener<ProgramList> listener);

        void Get3DEnabled(ResponseListener<bool> listener);
        void Set3DEnabled(bool enabled, ResponseListener<object> listener);
        IServiceSubscription<bool> Subscribe3DEnabled(ResponseListener<bool> listener);
    }
}