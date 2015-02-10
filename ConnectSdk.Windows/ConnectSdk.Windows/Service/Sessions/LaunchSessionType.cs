namespace ConnectSdk.Windows.Service.Sessions
{
    public enum LaunchSessionType
    {
        /// <summary>
        /// Unknown LaunchSession type, may be unable to close this launch session
        /// </summary>
        Unknown,
        /// <summary>
        /// LaunchSession represents a launched app
        /// </summary>
        App,
        /// <summary>
        /// LaunchSession represents an external input picker that was launched
        /// </summary>
        ExternalInputPicker,
        /// <summary>
        /// LaunchSession represents a media app
        /// </summary>
        Media,
        /// <summary>
        /// LaunchSession represents a web app
        /// </summary>
        WebApp
    }
}