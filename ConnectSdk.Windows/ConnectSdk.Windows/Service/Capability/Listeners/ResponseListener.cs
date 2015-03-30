using System;
using ConnectSdk.Windows.Service.Command;

namespace ConnectSdk.Windows.Service.Capability.Listeners
{
    public class ResponseListener<T>
    {
        //public EventHandler<object> Success;
        //public EventHandler<ServiceCommandError> Error;

        // funcs to be sent as parameters
        private readonly Action<T> onSuccessFunc;
        private readonly Action<ServiceCommandError> onErrorFunc;

        public ResponseListener() { }

        /// <summary>
        /// Specific constructor with actions to be executed on events
        /// </summary>
        /// <param name="onSuccess">The action to be executed on success</param>
        /// <param name="onError">The action to be executed on error</param>
        public ResponseListener(Action<T> onSuccess, Action<ServiceCommandError> onError)
        {
            onSuccessFunc = onSuccess;
            onErrorFunc = onError;
        }

        public void OnSuccess(T obj)
        {
            //if (Success != null)
            //    Success(this, new LoadEventArgs(obj));
            if (onSuccessFunc != null)
                onSuccessFunc(obj);
        }

        public void OnError(ServiceCommandError obj)
        {
            //if (Error != null)
            //    Error(this, obj);
            if (onErrorFunc != null)
                onErrorFunc(obj);
        }

       
    }

    //public interface IResponseListener<T>
    //{
    //    void OnSuccess(T loadArgument);
    //    void OnError(ServiceCommandError serviceCommandError);
    //}
}