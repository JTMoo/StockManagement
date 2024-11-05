using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Settings : BaseDocument
{
	private AvailableLanguages _selectedLanguage;


	public AvailableLanguages SelectedLanguage
	{
		get => this._selectedLanguage;
		internal set => this.SetField(ref this._selectedLanguage, value);
	}
}