using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IUserServiceProvider
{
	/// <returns><see langword="null"/> if no user has that id</returns>
	public Task<User?> GetUserAsync(string id);

	/// <returns><see langword="null"/> if no user has that username</returns>
	public Task<User?> GetUserByUsernameAsync(string username);
	public Task<IEnumerable<User>> GetAllUsersAsync();

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateUserAsync(User user);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteUserAsync(User user);
	public Task AddUserAsync(User user);
}
