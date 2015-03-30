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
            switch (code)
            {
                case 400:
                    break;
                case 401:
                    break;
                case 500:
                    break;
                case 503:
                    break;
            }

            return new ServiceCommandError(code, null);
        }
    }
}