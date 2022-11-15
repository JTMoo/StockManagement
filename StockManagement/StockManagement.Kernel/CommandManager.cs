namespace StockManagement.Kernel
{
    internal class CommandManager : IDisposable
    {
        private CancellationTokenSource _commandExecutionCancellation;
        private bool _disposed;


        public CommandManager() 
        {
            _commandExecutionCancellation = new CancellationTokenSource();
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (this._commandExecutionCancellation != null)
            {
                this._commandExecutionCancellation.Cancel();
                this._commandExecutionCancellation.Dispose();
            }

            _disposed = true;
        }

        internal void Init()
        {
            this.StartCommandExecutionTask();
        }

        private void StartCommandExecutionTask()
        {
            Task.Run(() =>
            {
                while(!this._commandExecutionCancellation.IsCancellationRequested)
                {

                }
            });
        }
    }
}