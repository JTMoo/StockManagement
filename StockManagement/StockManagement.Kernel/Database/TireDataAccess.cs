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
}
