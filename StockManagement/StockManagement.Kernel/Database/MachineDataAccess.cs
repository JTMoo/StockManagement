using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class MachineDataAccess
{
	internal static async Task<List<Machine>> GetAll()
	{
		var machineCollection = DatabaseManager.ConnectToMongo<Machine>(typeof(Machine).ToString());
		var machines = await machineCollection.FindAsync(_ => true);
		return machines.ToList();
	}

	internal static Task Add(Machine machine)
	{
		var col = DatabaseManager.ConnectToMongo<Machine>(typeof(Machine).ToString());
		return col.InsertOneAsync(machine);
	}

	internal static Task Update(Machine machine)
	{
		var col = DatabaseManager.ConnectToMongo<Machine>(typeof(Machine).ToString());
		var filter = Builders<Machine>.Filter.Eq("Id", machine.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, machine, new ReplaceOptions { IsUpsert = true });
	}
}
