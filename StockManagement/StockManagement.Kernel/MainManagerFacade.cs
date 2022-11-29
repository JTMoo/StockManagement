using StockManagement.Kernel.Commands;

namespace StockManagement.Kernel;


public class MainManagerFacade
{
	public bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}
}
