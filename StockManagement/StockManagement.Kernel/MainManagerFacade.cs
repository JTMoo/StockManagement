using System.Collections.ObjectModel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel;


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
	public static AvailableLanguages SelectedLanguage
	{
		get { return MainManager.Instance.Settings.SelectedLanguage; }
	}
	public static bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}
}
