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

	public async Task<int> DeleteUserAsync(User user)
	{
		var result = await _database.Delete<User>(user);
		return (int)result.DeletedCount;
	}

	public Task<IEnumerable<User>> GetAllUsersAsync()
	{
		return _database.GetAll<User>();
	}

	public Task<User> GetUserAsync(string id)
	{
		return _database.GetOneAsync<User>(user => user.Id == id);
	}

	public async Task<User?> GetUserByUsernameAsync(string username)
	{
		return await _database.GetOneAsync<User>(user => user.Username == username);
	}

	public async Task<int> UpdateUserAsync(User user)
	{
		var collection = _database.ConnectToMongo<User>();
		var filter = Builders<User>.Filter.Eq("Id", user.Id);
		// Upsert means: replace if existent - insert if not existent
		var result = await collection.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
		return (int)result.ModifiedCount;
	}
}
