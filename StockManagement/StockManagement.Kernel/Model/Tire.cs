namespace StockManagement.Kernel.Model;

internal class Tire : StockItem
{
	private Dimensions _dimensions;


	public class Dimensions
	{
		private readonly double _diameter = 0.0;
		private readonly double _profile = 0.0;


		public Dimensions (double diameter, double profile)
		{
			this._diameter = diameter;
			this._profile = profile;
		}

		public double Diameter
		{
			get { return this._diameter; }
		}

		public double Profile
		{
			get { return this._profile; }
		}
	}
}
