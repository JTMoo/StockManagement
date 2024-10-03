namespace StockManagement.Kernel.Model;


public class TireDimensions(double rimDiameter, double profile, double width)
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