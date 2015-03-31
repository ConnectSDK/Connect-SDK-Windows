using System;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability.Listeners
{
    public class ResponseListener
    {
        public EventHandler<object> Success;
        public EventHandler<ServiceCommandError> Error;
        
        // funcs to be sent as parameters
        private readonly Action<object> onSuccessFunc;
        private readonly Action<ServiceCommandError> onErrorFunc;

        public ResponseListener() { }

        /// <summary>
        /// Specific constructor with actions to be executed on events
        /// </summary>
        /// <param name="onSuccess">The action to be executed on success</param>
        /// <param name="onError">The action to be executed on error</param>
        public ResponseListener(Action<object> onSuccess, Action<ServiceCommandError> onError)
        {
            onSuccessFunc = onSuccess;
            onErrorFunc = onError;
        }

        public void OnSuccess(object obj)
        {
            if (Success != null)
                Success(this, new LoadEventArgs(obj));
            if (onSuccessFunc != null)
                onSuccessFunc(new LoadEventArgs(obj));
        }

        public void OnError(ServiceCommandError obj)
        {
            if (Error != null)
                Error(this, obj);
            if (onErrorFunc != null)
                onErrorFunc(obj);
        }

    }
}