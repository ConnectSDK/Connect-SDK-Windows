using System;
using System.Dynamic;
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