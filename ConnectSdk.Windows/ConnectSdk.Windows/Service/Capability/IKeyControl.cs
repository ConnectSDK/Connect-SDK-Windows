using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IKeyControl : IControl
    {
        IKeyControl GetKeyControl();
        CapabilityPriorityLevel GetKeyControlCapabilityLevel();

        void Up(ResponseListener listener);
        void Down(ResponseListener listener);
        void Left(ResponseListener listener);
        void Right(ResponseListener listener);
        void Ok(ResponseListener listener);
        void Back(ResponseListener listener);
        void Home(ResponseListener listener);
        void SendKeyCode(int keyCode, ResponseListener pListener);
    }
}