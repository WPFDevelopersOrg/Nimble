﻿using System;
using System.Windows.Input;

namespace Nimble.Helpers
{
    public class RelayCommand : ICommand
    {
        private Action<object> executeAction;
        private Func<object, bool> canExecuteFunc;
        public event EventHandler CanExecuteChanged;
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        { }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
            {
                return;
            }
            executeAction = execute;
            canExecuteFunc = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            if (canExecuteFunc == null)
            {
                return true;
            }
            return canExecuteFunc(parameter);
        }
        public void Execute(object parameter)
        {
            if (executeAction == null)
            {
                return;
            }
            executeAction(parameter);
        }
    }
}
