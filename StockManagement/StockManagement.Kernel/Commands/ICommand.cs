using StockManagement.Kernel.Commands.Data;

namespace StockManagement.Kernel.Commands;


public interface ICommand
{
    public CommandData Data { get; }
    public Task<bool> Execute();
}