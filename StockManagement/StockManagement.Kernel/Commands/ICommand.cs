using StockManagement.Kernel.Commands.Data;

namespace StockManagement.Kernel.Commands;


/// ********************************************************************************************************************************
/// <summary>
/// Interface for commands for inter-project communication
/// </summary>
/// ********************************************************************************************************************************
public interface ICommand
{
    public CommandData Data { get; }
    public Task<bool> Execute();
}