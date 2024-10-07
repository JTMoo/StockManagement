using System.ComponentModel.DataAnnotations;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.tireDimensions))]
public class TireDimensions(double rimDiameter, double profile, double width)
{
	private readonly double _rimDiameter = rimDiameter;
	private readonly double _profile = profile;
	private readonly double _width = width;


	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.rimDiameter))]
	public double RimDiameter
	{
		get { return this._rimDiameter; }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.profile))]
	public double Profile
	{
		get { return this._profile; }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.width))]
	public double Width
	{
		get { return this._width; }
	}
}