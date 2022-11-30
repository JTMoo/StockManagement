using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel;


public static class MainManagerFacade
{
	public static bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}
}
