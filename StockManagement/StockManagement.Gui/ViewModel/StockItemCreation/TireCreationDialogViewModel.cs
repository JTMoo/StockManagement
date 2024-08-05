using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Gui.ViewModel.StockItemCreation;


public class TireCreationDialogViewModel : DialogViewModelBase
{
	private string _description;
	private string _name;
	private int _price;
	private int _profile;
	private int _rimDiameter;
	private int _width;
	private ManufacturerType _selectedManufacturer;
	private int _amount;


	public TireCreationDialogViewModel() : base()
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

	public int Profile
	{
		get { return _profile; }
		set { this.SetField(ref _profile, value); }
	}

	public int RimDiameter
	{
		get { return _rimDiameter; }
		set { this.SetField(ref _rimDiameter, value); }
	}

	public int Width
	{
		get { return _width; }
		set { this.SetField(ref _width, value); }
	}
	#endregion

	public override void Confirm(string obj)
	{
		var command = new StockItemCreationCommand
		{
			Data = new StockItemCommandData
			{
				StockItem = new Tire(rimDiameter: this.RimDiameter, 
					profile: this.Profile, 
					width: this.Width,
					name: this.Name,
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