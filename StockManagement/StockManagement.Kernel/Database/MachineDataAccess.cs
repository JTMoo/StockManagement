using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal class MachineDataAccess
{
	internal async Task<List<Machine>> GetAll()
	{
		var machineCollection = DatabaseManager.ConnectToMongo<Machine>(typeof(Machine).ToString());
		var machines = await machineCollection.FindAsync(_ => true);
		return machines.ToList();
	}

	internal Task Add(Machine machine)
	{
		var col = DatabaseManager.ConnectToMongo<Machine>(typeof(Machine).ToString());
		return col.InsertOneAsync(machine);
	}
}
