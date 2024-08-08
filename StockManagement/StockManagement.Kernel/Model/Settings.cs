using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Settings : NotificationBase
{
	private Language _selectedLanguage;


	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public Language SelectedLanguage
	{
		get => this._selectedLanguage;
		internal set => this.SetField(ref this._selectedLanguage, value);
	}
}