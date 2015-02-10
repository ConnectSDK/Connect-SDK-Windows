namespace MyRemote.ConnectSDK.Service.Capability
{
    public class AppState
    {
        /** Whether the app is currently running. */
        public bool Running;
        /** Whether the app is currently visible. */
        public bool Visible;

        public AppState(bool running, bool visible)
        {
            Running = running;
            Visible = visible;
        }
    }
}