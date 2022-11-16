using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel
{
    internal class CommandManager : IDisposable
    {
        private CancellationTokenSource _commandExecutionCancellation;
        private bool _disposed;
        private CommandQueue queue;


        public CommandManager() 
        {
            _commandExecutionCancellation = new CancellationTokenSource();
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
            this.StartCommandExecutionTask();
        }

        internal void StopCommandExecution()
        {
            if (this._commandExecutionCancellation == null) return;

            this._commandExecutionCancellation.Cancel();
        }

        private void StartCommandExecutionTask()
        {
            Task.Run(() =>
            {
                while(!this._commandExecutionCancellation.IsCancellationRequested)
                {
                    var command = queue.Pop();
                    if (command == null) continue;

                    var result = command.Execute();
                    command.Data.InvokeCallback(result);
                }
            });
        }
    }
}