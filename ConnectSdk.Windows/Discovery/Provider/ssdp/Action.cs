#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Action.cs
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
using System.Collections.Generic;

namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Action
    {
        private String name;

        /// <summary>
        /// Gets or sets the name of the action
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the arguments of the action
        /// </summary>
        public List<Argument> ArgumentList { get; set; }

        public Action(String name)
        {
            Name = name;
        }

    }
}