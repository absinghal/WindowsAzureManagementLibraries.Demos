using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WamlDemos
{
    public class Commands
    {
        static Commands()
        {
            SubscriptionSelected = (Action<PublishSettingsSubscriptionItem>)RaiseSubscriptionSelected;
        }

        static void RaiseSubscriptionSelected(PublishSettingsSubscriptionItem item)
        {
            var handler = SubscriptionSelectedEvent;

            if (handler != null)
                handler(null, item);
        }

        public static event EventHandler<PublishSettingsSubscriptionItem> SubscriptionSelectedEvent;

        public static ActionCommand<PublishSettingsSubscriptionItem> SubscriptionSelected;
        public static RoutedCommand SetStatus = new RoutedCommand();
        public static RoutedCommand CreateNewStorageAccount = new RoutedCommand();
    }

    public class ActionCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Action<T> _execute;
        Func<T, bool> _canExecute;
        bool _couldExecute;

        public static implicit operator ActionCommand<T>(Action<T> execute)
        {
            return new ActionCommand<T>(execute);
        }

        public ActionCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (x => true);
            _couldExecute = true;
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            try
            {
                var canExecute = CanExecute((T)parameter);

                if (_couldExecute ^ canExecute)
                {
                    _couldExecute = canExecute;
                    OnCanExecuteChanged();
                }

                return canExecute;
            }
            catch (InvalidCastException)
            {
                if (_couldExecute)
                {
                    _couldExecute = false;
                    OnCanExecuteChanged();
                }

                return false;
            }
        }

        private void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, new EventArgs());
        }

        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }
    }
}
