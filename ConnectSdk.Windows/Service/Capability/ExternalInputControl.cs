#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ExternalInputControl.cs
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
namespace ConnectSdk.Windows.Service.Capability
{
    public class ExternalInputControl : CapabilityMethods
    {
        public static string Any = "ExternalInputControl.Any";

        public static string PickerLaunch = "ExternalInputControl.Picker.Launch";
        public static string PickerClose = "ExternalInputControl.Picker.Close";
        public static string List = "ExternalInputControl.List";
        public static string Set = "ExternalInputControl.Set";

        public static string[] Capabilities =
        {
            PickerLaunch,
            PickerClose,
            List,
            Set
        };
    }
}