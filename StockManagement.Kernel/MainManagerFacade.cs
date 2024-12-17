using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;


/// ********************************************************************************************************************************
/// <summary>
/// Publishes certain Objects of the MainManager to other projects
/// </summary>
/// ********************************************************************************************************************************
public static class MainManagerFacade
{
	public static Settings Settings
	{
		get { return MainManager.Instance.Settings; }
	}
	public static bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}
}
