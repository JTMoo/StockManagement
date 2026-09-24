using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


/// <summary>
/// Single row of application-wide settings
/// </summary>
public class AppSettings : BaseDocument
{
	private AvailableLanguages _language = AvailableLanguages.German;


	public AvailableLanguages Language
	{
		get => this._language;
		set => this.SetField(ref this._language, value);
	}
}
