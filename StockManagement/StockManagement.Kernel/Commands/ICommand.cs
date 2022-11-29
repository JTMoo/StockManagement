namespace StockManagement.Kernel.Commands
{
    public interface ICommand
    {
        public CommandData Data { get; }
        public bool Execute();
    }
}