using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;


public static class MainManagerFacade
{
	public static bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}
}
