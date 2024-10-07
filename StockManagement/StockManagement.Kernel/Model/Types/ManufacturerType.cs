using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockManagement.Kernel.Model.Types;


public enum ManufacturerType
{
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.none))]
	[Description("")]
	None = 0,

	[Description("Samasz")]
	Samasz,

	[Description("Metallfach")]
	Metallfach,

	[Description("Hattat")]
	Hattat
}
