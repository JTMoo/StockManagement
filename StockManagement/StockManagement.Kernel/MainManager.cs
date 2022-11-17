using System.Diagnostics;

namespace StockManagement.Kernel
{
    public class MainManager : IDisposable
    {
        public static readonly MainManager Instance = new MainManager();

        private CommandManager _commandManager;

        public MainManager() 
        {
            _commandManager = new CommandManager();
        }

        ~MainManager()
        {
            this.Dispose();
        }

        public void Init()
        {
            this._commandManager.Init();
        }

        public void Dispose()
        {
            this._commandManager.Dispose();
        }

        public async void StartObservedTask(Action action)
        {
            try
            {
                await Task.Factory.StartNew(action);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public async void StartObservedTask(Action action, CancellationToken token)
        {
            try
            {
                await Task.Factory.StartNew(action, token);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}