using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class SparePartDataAccess
{
	internal static async Task<List<SparePart>> GetAll()
	{
		var sparePartCollection = DatabaseManager.ConnectToMongo<SparePart>(typeof(SparePart).ToString());
		var spareParts = await sparePartCollection.FindAsync(_ => true);
		return spareParts.ToList();
	}

	internal static Task Add(SparePart sparePart)
	{
		var col = DatabaseManager.ConnectToMongo<SparePart>(typeof(SparePart).ToString());
		return col.InsertOneAsync(sparePart);
	}

	internal static Task Update(SparePart sparePart)
	{
		var col = DatabaseManager.ConnectToMongo<SparePart>(typeof(SparePart).ToString());
		var filter = Builders<SparePart>.Filter.Eq("Id", sparePart.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, sparePart, new ReplaceOptions { IsUpsert = true });
	}
}
