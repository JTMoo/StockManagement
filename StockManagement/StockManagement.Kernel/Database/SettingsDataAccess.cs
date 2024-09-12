using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class SettingsDataAccess
{
	internal static Settings Get()
	{
		var settingsCollection = DatabaseManager.ConnectToMongo<Settings>(typeof(Settings).ToString());
		var settings = settingsCollection.Find(_ => true);
		return settings.FirstOrDefault();
	}

	internal static Task Update()
	{
		var col = DatabaseManager.ConnectToMongo<Settings>(typeof(Settings).ToString());
		var filter = Builders<Settings>.Filter.Eq("Id", MainManager.Instance.Settings.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, MainManager.Instance.Settings, new ReplaceOptions { IsUpsert = true });
	}
}
