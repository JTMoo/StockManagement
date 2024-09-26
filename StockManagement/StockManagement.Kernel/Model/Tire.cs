using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Tire : StockItem
{
	public Tire() : base()
	{
	}

	public Tire (double rimDiameter, double profile, double width, string name, string code = "", string description = "", int amount = 1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None) 
		: base (name:name, code:code, description:description, amount:amount, price:price, manufacturer:manufacturer)
	{
		this.Data = new Dimensions(rimDiameter, profile, width);
	}

	public Dimensions Data { get; set; }

	internal override void Register()
	{
		MainManager.Instance.TireManager.Register(this);
	}

	internal override void Update(Action callback)
	{
		MainManager.Instance.TireManager.Update(this, callback);
	}

	public class Dimensions(double rimDiameter, double profile, double width)
	{
		private readonly double _rimDiameter = rimDiameter;
		private readonly double _profile = profile;
		private readonly double _width = width;

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