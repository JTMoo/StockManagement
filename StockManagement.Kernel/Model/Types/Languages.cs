using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockManagement.Kernel.Model.Types;


public enum AvailableLanguages
{
	[Display(ResourceType = typeof(Language.Settings), Name = nameof(Language.Settings.german))]
	[Description("de-DE")]
	German,

	[Display(ResourceType = typeof(Language.Settings), Name = nameof(Language.Settings.english))]
	[Description("en-US")]
	English,

	[Display(ResourceType = typeof(Language.Settings), Name = nameof(Language.Settings.spanish))]
	[Description("es-PY")]
	Spanish
}