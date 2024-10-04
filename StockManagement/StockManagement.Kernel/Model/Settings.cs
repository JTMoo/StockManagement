using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Settings : NotificationBase
{
	private AvailableLanguages _selectedLanguage;


	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public AvailableLanguages SelectedLanguage
	{
		get => this._selectedLanguage;
		internal set => this.SetField(ref this._selectedLanguage, value);
	}
}