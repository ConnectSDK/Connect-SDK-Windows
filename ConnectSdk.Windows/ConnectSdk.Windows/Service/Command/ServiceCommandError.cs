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