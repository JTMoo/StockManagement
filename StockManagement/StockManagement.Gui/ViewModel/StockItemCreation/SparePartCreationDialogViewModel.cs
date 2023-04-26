using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.StockItemCreation;


public class SparePartCreationDialogViewModel : DialogViewModelBase
{
	private string _description;
	private string _location;
	private string _name;
	private int _price;
	private ManufacturerType _selectedManufacturer;


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

	public int Price
	{
		get { return _price; }
		set { this.SetField(ref _price, value); }
	}
	#endregion

	public override void Confirm(string obj)
	{
		var sparePart = new SparePart(name: this.Name, description: this.Description, price: this.Price, manufacturer: this.SelectedManufacturer);
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = sparePart,
				Callback = success => GuiManager.Instance.MainViewModel.StockItemCreationFinished(success, sparePart)
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}
