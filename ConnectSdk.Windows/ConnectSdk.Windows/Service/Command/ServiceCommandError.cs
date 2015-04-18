#region Copyright Notice
/*
 * ConnectSdk.Windows
 * ServiceCommandError.cs
 * 
 * Copyright (c) 2015 LG Electronics.
 * Created by Sorin S. Serban on 20-2-2015,
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

namespace ConnectSdk.Windows.Service.Command
{
    public class ServiceCommandError
    {
        private readonly int code;
        private readonly Object payload;

        public static ServiceCommandError NotSupported()
        {
            return new ServiceCommandError(503, null);
        }

        public ServiceCommandError(int code, Object payload) 
        {
            this.code = code;
            this.payload = payload;
        }

        public int GetCode()
        {
            return code;
        }

        public Object GetPayload()
        {
            return payload;
        }

        public static ServiceCommandError GetError(int code)
        {
            string desc = null;
            if (code == 400)
            {
                desc = "Bad Request";
            }
            else if (code == 401)
            {
                desc = "Unauthorized";
            }
            else if (code == 500)
            {
                desc = "Internal Server Error";
            }
            else if (code == 503)
            {
                desc = "Service Unavailable";
            }
            else
            {
                desc = "Unknown Error";
            }

            return new ServiceCommandError(code, null);
        }
    }
}