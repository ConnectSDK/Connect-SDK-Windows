#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Argument.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 22-4-2015,
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
 #endregion
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