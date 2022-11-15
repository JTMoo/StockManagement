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

        public void Init()
        {
            this._commandManager.Init();
        }

        public void Dispose()
        {
            this._commandManager.Dispose();
        }
    }
}