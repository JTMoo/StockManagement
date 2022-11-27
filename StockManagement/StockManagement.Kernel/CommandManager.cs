using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel;

internal class CommandManager : IDisposable
{
    private CancellationTokenSource _commandExecutionCancellation;
    private bool _disposed;
    private CommandQueue queue = new CommandQueue();


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

        if (this._commandExecutionCancellation != null)
        {
            this._commandExecutionCancellation.Dispose();
        }

        _disposed = true;
    }

    internal void Init()
    {
        this.StartCommandExecutionTask(this._commandExecutionCancellation.Token);
    }

    internal void StopCommandExecution()
    {
        if (this._commandExecutionCancellation == null) return;

        this._commandExecutionCancellation.Cancel();
    }

    private void StartCommandExecutionTask(CancellationToken cancellationToken)
    {
        MainManager.Instance.StartObservedTask(() =>
        {
            while(!this._commandExecutionCancellation.IsCancellationRequested)
            {
                var command = queue.Pop();
                if (command == null) continue;

                var result = command.Execute();
                command.Data.InvokeCallback(result);
            }
        }, cancellationToken);
    }
}