using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IUserServiceProvider
{
	public Task<User> GetUserAsync(string id);
	public Task<IEnumerable<User>> GetAllUsersAsync();
	public Task<ReplaceOneResult> UpdateUserAsync(User user);
	public Task<DeleteResult> DeleteUserAsync(User user);
	public Task AddUserAsync(User user);
}
