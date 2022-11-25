namespace StockManagement.Kernel.Model;

internal class Tire : StockItem
{
	private Dimensions _dimensions;


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
