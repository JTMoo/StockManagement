using StockManagement.Kernel;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Commands.StockItemCommands;

namespace StockManagement.Gui.ViewModel.StockItemCreation;


public class MachineCreationDialogViewModel : DialogViewModelBase
{
	private string _description;
	private string _name;
	private int _price;
	private ManufacturerType _selectedManufacturer;
	private int _amount;

	public MachineCreationDialogViewModel() : base()
	{
	}

	#region Properties
	public string Description
	{
		get { return _description; }
		set { this.SetField(ref _description, value); }
	}

	public int Amount
	{
		get { return _amount; }
		set { this.SetField(ref _amount, value); }
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
		var command = new StockItemCreationCommand
		{
			Data = new StockItemCommandData
			{
				DataToRegister = new Machine(name: this.Name,
					description: this.Description,
					amount: this.Amount,
					price: this.Price,
					manufacturer: this.SelectedManufacturer)
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}