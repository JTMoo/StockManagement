using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class TireDataAccess
{
	internal async Task<List<Tire>> GetAll()
	{
		var tireCollection = DatabaseManager.ConnectToMongo<Tire>(typeof(Tire).ToString());
		var tires = await tireCollection.FindAsync(_ => true);
		return tires.ToList();
	}

	internal Task Add(Tire tire)
	{
		var col = DatabaseManager.ConnectToMongo<Tire>(typeof(Tire).ToString());
		return col.InsertOneAsync(tire);
	}

	internal Task Update(Tire tire)
	{
		var col = DatabaseManager.ConnectToMongo<Tire>(typeof(Tire).ToString());
		var filter = Builders<Tire>.Filter.Eq("Id", tire.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, tire, new ReplaceOptions { IsUpsert = true });
	}
}
