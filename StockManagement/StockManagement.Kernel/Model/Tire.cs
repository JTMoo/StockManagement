using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Tire : StockItem
{
	public Tire() : base()
	{
	}

	public Tire (double rimDiameter, double profile, double width, string name, string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None) 
		: base (name:name, description:description, amount:amount, price:price, manufacturer:manufacturer)
	{
		this.Data = new Dimensions(rimDiameter, profile, width);
	}

	public Dimensions Data { get; set; }

	internal override void Register()
	{
		MainManager.Instance.TireManager.Register(this);
	}

	public class Dimensions
	{
		private readonly double _rimDiameter = 0.0;
		private readonly double _profile = 0.0;
		private readonly double _width = 0.0;


		public Dimensions (double rimDiameter, double profile, double width)
		{
			this._rimDiameter = rimDiameter;
			this._profile = profile;
			this._width = width;
		}

		public double RimDiameter
		{
			get { return this._rimDiameter; }
		}

		public double Profile
		{
			get { return this._profile; }
		}

		public double Width
		{
			get { return this._width; }
		}
	}
}