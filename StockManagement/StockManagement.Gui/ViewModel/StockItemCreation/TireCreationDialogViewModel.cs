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
		var tire = new Tire(rimDiameter: this.RimDiameter, profile: this.Profile, width: this.Width,
			name: this.Name, description: this.Description, price: this.Price, manufacturer: this.SelectedManufacturer);
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = tire,
				Callback = success => GuiManager.Instance.MainViewModel.StockItemCreationFinished(success, tire)
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}