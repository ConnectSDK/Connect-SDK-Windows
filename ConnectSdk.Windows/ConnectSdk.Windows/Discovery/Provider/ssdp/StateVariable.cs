namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class StateVariable
    {
        // ReSharper disable InconsistentNaming
        public static string TAG = "stateVariable";
        public static string TAG_NAME = "name";
        public static string TAG_DATA_TYPE = "dataType";
        // ReSharper restore InconsistentNaming

        public StateVariable()
        {
            Multicast = "no";
            SendEvents = "yes";
        }

        /// <summary>
        /// Optional. Defines whether event messages will be generated when the value
        /// of this state variable changes. Defaut value is "yes".
        /// </summary>
        public string SendEvents { get; set; }

        /// <summary>
        /// Optional. Defines whether event messages will be delivered using
        /// multicast eventing. Default value is "no".
        /// </summary>
        public string Multicast { get; set; }

        /// <summary>
        /// Required. Name of state variable.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Required. Same as data types defined by XML Schema.
        /// </summary>
        public string DataType { get; set; }
    }
}