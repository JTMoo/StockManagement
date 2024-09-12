using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class TireDataAccess
{
	internal static async Task<List<Tire>> GetAll()
	{
		var tireCollection = DatabaseManager.ConnectToMongo<Tire>(typeof(Tire).ToString());
		var tires = await tireCollection.FindAsync(_ => true);
		return tires.ToList();
	}

	internal static Task Add(Tire tire)
	{
		var col = DatabaseManager.ConnectToMongo<Tire>(typeof(Tire).ToString());
		return col.InsertOneAsync(tire);
	}

	internal static Task Update(Tire tire)
	{
		var col = DatabaseManager.ConnectToMongo<Tire>(typeof(Tire).ToString());
		var filter = Builders<Tire>.Filter.Eq("Id", tire.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, tire, new ReplaceOptions { IsUpsert = true });
	}
}
