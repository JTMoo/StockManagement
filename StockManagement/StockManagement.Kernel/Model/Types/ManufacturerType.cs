using System.ComponentModel;

namespace StockManagement.Kernel.Model.Types;


public enum ManufacturerType
{
	[Description("")]
	None = 0,

	[Description("Samasz")]
	Samasz,

	[Description("Metallfach")]
	Metallfach,

	[Description("Hattat")]
	Hattat
}
