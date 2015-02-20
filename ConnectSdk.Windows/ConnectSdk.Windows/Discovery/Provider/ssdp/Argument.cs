using System;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Argument
    {
        // ReSharper disable InconsistentNaming
        public static String TAG = "argument";
        public static String TAG_NAME = "name";
        public static String TAG_DIRECTION = "direction";
        public static String TAG_RETVAL = "retval";
        public static String TAG_RELATED_STATE_VARIABLE = "relatedStateVariable";
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Required. Name of formal parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Required. Defines whether argument is an input or output paramter.
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// Optional. Identifies at most one output argument as the return value.
        /// </summary>
        public string Retval { get; set; }

        /// <summary>
        /// Required. Must be the same of a state variable.
        /// </summary>
        public string RelatedStateVariable { get; set; }
    }
}