using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IUserServiceProvider
{
	/// <returns><see langword="null"/> if no user has that id</returns>
	public Task<User?> GetUserAsync(string id, CancellationToken cancellationToken = default);

	/// <returns><see langword="null"/> if no user has that username</returns>
	public Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
	public Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);

	/// <summary>Users by <see cref="User.Username"/> then <see cref="BaseDocument.Id"/>, one page at a time</summary>
	public Task<CursorPage<User>> GetUsersAsync(string? cursor, int pageSize, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateUserAsync(User user, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteUserAsync(User user, CancellationToken cancellationToken = default);
	public Task AddUserAsync(User user, CancellationToken cancellationToken = default);
}
