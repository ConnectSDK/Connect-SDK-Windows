namespace ConnectSdk.Windows.Service.Capability
{
    /// <summary>
    /// Helper class used with the AppStateListener to return the current state of an app.
    /// </summary>
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