using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class TransactionDataAccess
{
	internal async Task<List<Transaction>> GetAll()
	{
		var transactionCollection = DatabaseManager.ConnectToMongo<Transaction>(typeof(Transaction).ToString());
		var tires = await transactionCollection.FindAsync(_ => true);
		return tires.ToList();
	}

	internal Task Add(Transaction transaction)
	{
		var col = DatabaseManager.ConnectToMongo<Transaction>(typeof(Transaction).ToString());
		return col.InsertOneAsync(transaction);
	}
}
