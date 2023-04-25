using StockManagement.Kernel.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.StockItemCreation;

public class MachineCreationDialogViewModel : DialogViewModelBase
{
	private string _description;
	private string _name;
	private int _price;
	private ManufacturerType _selectedManufacturer;


	public MachineCreationDialogViewModel() : base()
	{
	}

	#region Properties
	public string Description
	{
		get { return _description; }
		set { this.SetField(ref _description, value); }
	}
	public ManufacturerType SelectedManufacturer
	{
		get { return _selectedManufacturer; }
		set { this.SetField(ref _selectedManufacturer, value); }
	}

	public string Name
	{
		get { return _name; }
		set { this.SetField(ref _name, value); }
	}

	public int Price
	{
		get { return _price; }
		set { this.SetField(ref _price, value); }
	}
	#endregion

	public override void Confirm(string obj)
	{
		var machine = new Machine(name: this.Name, 
			description: this.Description, price: this.Price, manufacturer: this.SelectedManufacturer);
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = machine,
				Callback = success => this.MachineCreationFinished(success, machine)
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}

	private void MachineCreationFinished(bool success, Machine machine)
	{
		if (!success) return;

		lock (GuiManager.Instance.MainViewModel.StockItemsLock)
		{
			GuiManager.Instance.MainViewModel.StockItems.Add(machine);
		}
	}
}
