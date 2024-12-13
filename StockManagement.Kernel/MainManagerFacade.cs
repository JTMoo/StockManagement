using System.Collections.ObjectModel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;


/// ********************************************************************************************************************************
/// <summary>
/// Publishes certain Objects of the MainManager to other projects
/// </summary>
/// ********************************************************************************************************************************
public static class MainManagerFacade
{
	public static ReadOnlyObservableCollection<Tire> Tires
	{
		get { return MainManager.Instance.TireManager.Tires; }
	}
	public static ReadOnlyObservableCollection<SparePart> SpareParts
	{
		get { return MainManager.Instance.SparePartManager.SpareParts; }
	}
	public static ReadOnlyObservableCollection<Machine> Machines
	{
		get { return MainManager.Instance.MachineManager.Machines; }
	}
	public static IDatabase Database
	{
		get { return MainManager.Instance.DatabaseManager; }
	}
	public static Settings Settings
	{
		get { return MainManager.Instance.Settings; }
	}
	public static bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}
}
