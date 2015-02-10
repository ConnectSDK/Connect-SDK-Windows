namespace ConnectSdk.Windows.Service.Sessions
{
    public enum WebAppStatus
    {
        /// <summary>
        /// Web app status is unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Web app is running and in the foreground
        /// </summary>
        Open,
        /// <summary>
        /// Web app is running and in the background
        /// </summary>
        Background,
        /// <summary>
        /// Web app is in the foreground but has not started running yet
        /// </summary>
        Foreground,
        /// <summary>
        /// Web app is not running and is not in the foreground or background
        /// </summary>
        Closed
    }
}