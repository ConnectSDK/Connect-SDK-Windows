namespace ConnectSdk.Windows.Core.Upnp.Service
{
    public class Service
    {
        // ReSharper disable InconsistentNaming
        public static string TAG = "service";
        public static string TAG_SERVICE_TYPE = "serviceType";
        public static string TAG_SERVICE_ID = "serviceId";
        public static string TAG_SCPD_URL = "SCPDURL";
        public static string TAG_CONTROL_URL = "controlURL";
        public static string TAG_EVENTSUB_URL = "eventSubURL";
        // ReSharper restore InconsistentNaming

        public string BaseUrl;

        /// <summary>
        /// Required. UPnP service type. 
        /// </summary>
        public string ServiceType;

        /// <summary>
        /// Required. Service identifier.  
        /// </summary>
        public string ServiceId;

        /// <summary>
        /// Required. Relative URL for service description. 
        /// </summary>
        public string ScpdUrl;

        /// <summary>
        /// Required. Relative URL for control. 
        /// </summary>
        public string ControlUrl;

        /// <summary>
        /// Relative. Relative URL for eventing. 
        /// </summary>
        public string EventSubUrl;
    }
}