using System;
using System.Windows.Input;

namespace StockManagement.Gui.Commands;

public class RelayCommand<T> : ICommand
{
    #pragma warning disable CA8600, CA8604
    readonly Action<T>? _execute;
    readonly Predicate<T>? _canExecute;


    public RelayCommand(Action<T> execute) : this(execute, _ => true)
    {
    }

    public RelayCommand(Action<T> execute, Predicate<T> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
            return false;
        if (parameter == null)
            return _canExecute(default!);
        return _canExecute((T)parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object? parameter)
    {
        if (_execute == null)
            return;
        if (parameter == null)
            _execute(default!);
        else
            _execute.Invoke((T)parameter);
    }
}
