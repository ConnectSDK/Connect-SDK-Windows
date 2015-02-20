using System;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Argument
    {
        public static String Tag = "argument";
        public static String TagName = "name";
        public static String TagDirection = "direction";
        public static String TagRetval = "retval";
        public static String TagRelatedStateVariable = "relatedStateVariable";

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