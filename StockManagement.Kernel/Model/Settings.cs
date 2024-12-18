using MongoDB.Driver;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


/// ********************************************************************************************************************************
/// <summary>
/// Model-class containing all information regarding Settings
/// </summary>
/// ********************************************************************************************************************************
public class Settings : BaseDocument
{
	private AvailableLanguages _selectedLanguage;
	private int _dialogBorderThickness = 100;


	public Settings()
	{
	}

	public Settings(AvailableLanguages selectedLanguage, int dialogBorderThickness)
	{
		this.SelectedLanguage = selectedLanguage;
		this.DialogBorderThickness = dialogBorderThickness;
	}

	public bool StockItemsIndexCreated { get; set; } = false;

	public AvailableLanguages SelectedLanguage
	{
		get => this._selectedLanguage;
		internal set => this.SetField(ref this._selectedLanguage, value);
	}
	
	public int DialogBorderThickness
	{
		get => this._dialogBorderThickness;
		internal set => this.SetField(ref this._dialogBorderThickness, value);
	}

	internal Task Update(Settings settings)
	{
		foreach (var pi in this.GetType().GetProperties())
		{
			pi.SetValue(this, pi.GetValue(settings));
		}

		var collection = MainManager.Instance.DatabaseManager.ConnectToMongo<Settings>();
		var filter = Builders<Settings>.Filter.Eq("Id", this.Id);
		return collection.ReplaceOneAsync(filter, this, new ReplaceOptions { IsUpsert = true });
	}
}