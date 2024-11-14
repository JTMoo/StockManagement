using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel;


internal class CommandManager : IDisposable
{
    private readonly CancellationTokenSource _commandExecutionCancellation;
    private bool _disposed;
    private readonly CommandQueue _queue = new();


    public CommandManager() 
    {
        this._commandExecutionCancellation = new CancellationTokenSource();
    }

    ~CommandManager()
    {
        this.Dispose();
    }

    public void Dispose()
    {
        if (this._disposed) return;
		this._commandExecutionCancellation?.Dispose();
		this._disposed = true;
		GC.SuppressFinalize(this);
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

    internal bool Push(ICommand command)
    {
        if (this._disposed) return false;
        if (this._commandExecutionCancellation == null) return false;
        if(command == null) return false;
        if (this._queue == null) return false;

		return this._queue.Add(command);
    }

    private void StartCommandExecutionTask(CancellationToken cancellationToken)
    {
		MainManager.StartObservedTask(async () =>
        {
            while(!this._commandExecutionCancellation.IsCancellationRequested)
            {
                Thread.Sleep(500);
                var command = _queue.Pop();
                if (command == null) continue;

                var result = await command.Execute();
                command.Data.InvokeCallback(result);
            }
        }, cancellationToken);
    }
}