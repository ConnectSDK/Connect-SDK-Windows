using System;
using System.Windows.Input;

namespace MyRemote.Tablet.Model
{
    public class Command : ICommand
    {
        private readonly Action<object> executeAction;
        public event EventHandler CanExecuteChanged;
        private bool enabled;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;

                    if (CanExecuteChanged != null)
                        CanExecuteChanged(this, new EventArgs());
                }
            }
        }

        public Command(Action<object> executeAction)
        {
            this.executeAction = executeAction;
        }

        public bool CanExecute(object parameter)
        {
            return enabled;
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}