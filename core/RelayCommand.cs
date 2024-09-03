using System;
using System.Windows.Input;

namespace SatisfactoryCalculatorGUI.core
{
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Predicate<object> canExecute;

        public RelayCommand(Predicate<object> canExecute, Action<object> execute) =>
            (this.canExecute, this.execute) = (canExecute, execute);

        public RelayCommand(Action<object> execute) : this(null, execute) { }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => this.canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => this.execute?.Invoke(parameter);
    }
}
