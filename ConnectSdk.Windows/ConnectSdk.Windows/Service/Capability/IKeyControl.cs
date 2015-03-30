using ConnectSdk.Windows.Service.Capability.Listeners;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IKeyControl : IControl
    {
        IKeyControl GetKeyControl();
        CapabilityPriorityLevel GetKeyControlCapabilityLevel();

        void Up(ResponseListener<object> listener);
        void Down(ResponseListener<object> listener);
        void Left(ResponseListener<object> listener);
        void Right(ResponseListener<object> listener);
        void Ok(ResponseListener<object> listener);
        void Back(ResponseListener<object> listener);
        void Home(ResponseListener<object> listener);
        void SendKeyCode(int keyCode, ResponseListener<object> pListener);
    }
}