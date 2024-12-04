using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockManagement.Kernel.Model.Types;


public enum AvailableLanguages
{
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.german))]
	[Description("de-DE")]
	German,

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.english))]
	[Description("en-US")]
	English,

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.spanish))]
	[Description("es-PY")]
	Spanish
}