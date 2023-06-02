using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class SparePartDataAccess
{
	internal async Task<List<SparePart>> GetAll()
	{
		var sparePartCollection = DatabaseManager.ConnectToMongo<SparePart>(typeof(SparePart).ToString());
		var spareParts = await sparePartCollection.FindAsync(_ => true);
		return spareParts.ToList();
	}

	internal Task Add(SparePart sparePart)
	{
		var col = DatabaseManager.ConnectToMongo<SparePart>(typeof(SparePart).ToString());
		return col.InsertOneAsync(sparePart);
	}
}
