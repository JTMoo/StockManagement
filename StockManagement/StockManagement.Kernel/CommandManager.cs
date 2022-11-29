using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel;

internal class CommandManager : IDisposable
{
    private CancellationTokenSource _commandExecutionCancellation;
    private bool _disposed;
    private CommandQueue _queue = new CommandQueue();


    public CommandManager() 
    {
        _commandExecutionCancellation = new CancellationTokenSource();
    }

    ~CommandManager()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_commandExecutionCancellation != null)
        {
            _commandExecutionCancellation.Dispose();
        }

        _disposed = true;
    }

    internal void Init()
    {
        this.StartCommandExecutionTask(_commandExecutionCancellation.Token);
    }

    internal void StopCommandExecution()
    {
        if (_commandExecutionCancellation == null) return;

        _commandExecutionCancellation.Cancel();
    }

    private void StartCommandExecutionTask(CancellationToken cancellationToken)
    {
        MainManager.Instance.StartObservedTask(() =>
        {
            while(!_commandExecutionCancellation.IsCancellationRequested)
            {
                var command = _queue.Pop();
                if (command == null) continue;

                var result = command.Execute();
                command.Data.InvokeCallback(result);
            }
        }, cancellationToken);
    }
}