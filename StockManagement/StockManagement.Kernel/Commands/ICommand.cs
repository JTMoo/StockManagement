namespace StockManagement.Kernel.Commands
{
    internal interface ICommand
    {
        public CommandData Data { get; }
        public bool Execute();
    }
}