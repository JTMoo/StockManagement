using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class UserServiceProvider(IDatabase database) : IUserServiceProvider
{
	private readonly IDatabase _database = database;


	public Task AddUserAsync(User user)
	{
		var collection = _database.ConnectToMongo<User>();
		return collection.InsertOneAsync(user);
	}

	public Task<DeleteResult> DeleteUserAsync(User user)
	{
		return _database.Delete<User>(user);
	}

	public Task<IEnumerable<User>> GetAllUsersAsync()
	{
		return _database.GetAll<User>();
	}

	public Task<User> GetUserAsync(string id)
	{
		return _database.GetOneAsync<User>(user => user.Id == id);
	}

	public Task<ReplaceOneResult> UpdateUserAsync(User user)
	{
		var collection = _database.ConnectToMongo<User>();
		var filter = Builders<User>.Filter.Eq("Id", user.Id);
		// Upsert means: replace if existent - insert if not existent
		return collection.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
	}
}
