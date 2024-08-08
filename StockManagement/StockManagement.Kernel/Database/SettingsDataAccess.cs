using MongoDB.Driver;
using StockManagement.Kernel.Model;
using System.Reflection.PortableExecutable;

namespace StockManagement.Kernel.Database;


internal class SettingsDataAccess
{
	internal Settings Get()
	{
		var settingsCollection = DatabaseManager.ConnectToMongo<Settings>(typeof(Settings).ToString());
		var settings = settingsCollection.Find(_ => true);
		return settings.FirstOrDefault();
	}

	internal Task Update()
	{
		var col = DatabaseManager.ConnectToMongo<Settings>(typeof(Settings).ToString());
		var filter = Builders<Settings>.Filter.Eq("Id", MainManager.Instance.Settings.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, MainManager.Instance.Settings, new ReplaceOptions { IsUpsert = true });
	}
}
