using System;
using System.Windows.Input;

namespace StockManagement.Gui.Commands;


public class RelayCommand<T>(Action<T> execute, Predicate<T> canExecute) : ICommand
{
	readonly Action<T>? _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    readonly Predicate<T>? _canExecute = canExecute;


    public RelayCommand(Action<T> execute) : this(execute, _ => true)
    {
    }

	public bool CanExecute(object? parameter)
    {
        if (this._canExecute == null)
            return false;
        if (parameter == null)
            return this._canExecute(default!);
        return this._canExecute((T)parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object? parameter)
    {
        if (this._execute == null)
            return;
        if (parameter == null)
			this._execute(default!);
        else
			this._execute.Invoke((T)parameter);
    }
}