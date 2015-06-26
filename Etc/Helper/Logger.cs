#region Copyright Notice
/*
 * ConnectSdk.Windows
 * Logger.cs
 * 
 * Copyright (c) 2015, https://github.com/sdaemon
 * Created by Sorin S. Serban on 25-4-2015,
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
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace ConnectSdk.Windows.Etc.Helper
{
    public class Logger
    {
        public EventHandler<object> Logged;
        private StringBuilder logContent;

        private static Logger current;
        public static Logger Current 
        {
            get
            {
                if (current == null)
                    current = new Logger();
                return current;
            }
        }

        public StringBuilder LogContent
        {
            get { return logContent; }
            set { logContent = value; }
        }

        public void AddMessage(string message)
        {
            var timeStammpedMessage = string.Format("{0} - {1}", DateTime.Now.ToString("yyyyMMdd-HHmmssTzz"), message);
            logContent.AppendLine(timeStammpedMessage);
            System.Diagnostics.Debug.WriteLine(timeStammpedMessage);
            if (Logged != null)
                Logged(this, timeStammpedMessage);
        }

        public Logger()
        {
            logContent = new StringBuilder();
        }
    }
}