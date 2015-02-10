namespace ConnectSdk.Windows.Service.Capability
{
    public class AppState
    {
        /// <summary>
        /// Whether the app is currently running.
        /// </summary>
        public bool Running;

        /// <summary>
        /// Whether the app is currently visible.
        /// </summary>
        public bool Visible;

        public AppState(bool running, bool visible)
        {
            Running = running;
            Visible = visible;
        }
    }
}