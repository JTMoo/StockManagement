﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StockManagement.Kernel.Tests")]
namespace StockManagement.Kernel.Commands;


internal class CommandQueue
{
    List<ICommand> _queue = new List<ICommand>();

    public bool Add (ICommand command)
    {
        if (command == null) return false;
        if (_queue == null) return false;

        _queue.Add(command);
        return true;
    }

    public ICommand? Pop ()
    {
        if (_queue == null) return null;
        if (_queue.Count == 0) return null;
        var command = _queue[0];
        _queue.RemoveAt(0);

		return command;
    }
}