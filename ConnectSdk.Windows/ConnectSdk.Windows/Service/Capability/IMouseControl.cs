using Windows.Foundation;

namespace ConnectSdk.Windows.Service.Capability
{
    public interface IMouseControl
    {
        IMouseControl GetMouseControl();
        CapabilityPriorityLevel GetMouseControlCapabilityLevel();

        void ConnectMouse();
        void DisconnectMouse();
        void Click();
        void Move(double dx, double dy);
        void Move(Point distance);
        void Scroll(double dx, double dy);
        void Scroll(Point distance);
    }
}