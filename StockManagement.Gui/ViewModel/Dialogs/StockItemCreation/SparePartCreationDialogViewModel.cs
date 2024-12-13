using StockManagement.Kernel;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Commands.StockItemCommands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.Dialogs.StockItemCreation;


public class SparePartCreationDialogViewModel : DialogViewModelBase
{
	private string _description;
	private string _location;
	private string _name;
	private string _code;
	private int _price;
	private ManufacturerType _selectedManufacturer;
	private int _amount;


	public SparePartCreationDialogViewModel() : base()
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

	public string Location
	{
		get { return _location; }
		set { this.SetField(ref _location, value); }
	}

	public string Name
	{
		get { return _name; }
		set { this.SetField(ref _name, value); }
	}

	public string Code
	{
		get { return _code; }
		set { this.SetField(ref _code, value); }
	}

	public int Amount
	{
		get { return _amount; }
		set { this.SetField(ref _amount, value); }
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
				DataToRegister = new SparePart(name: this.Name,
					description: this.Description,
					code: this.Code,
					location: this.Location,
					amount: this.Amount,
					price: this.Price,
					manufacturer: this.SelectedManufacturer)
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}