using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="IUserServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
public class EfUserServiceProvider(AppDbContext db) : IUserServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<User> GetUserAsync(string id)
	{
		return _db.Users.SingleAsync(user => user.Id == id);
	}

	public Task<User?> GetUserByUsernameAsync(string username)
	{
		return _db.Users.SingleOrDefaultAsync(user => user.Username == username);
	}

	public async Task<IEnumerable<User>> GetAllUsersAsync()
	{
		return await _db.Users.ToListAsync();
	}

	/// <exception cref="UsernameAlreadyExistsException">Username already in use</exception>
	public async Task AddUserAsync(User user)
	{
		_db.Users.Add(user);
		await this.SaveChangesAsync();
	}

	/// <exception cref="UsernameAlreadyExistsException">Username already in use</exception>
	public async Task<int> UpdateUserAsync(User user)
	{
		_db.Users.Update(user);
		await this.SaveChangesAsync();
		return 1;
	}

	public async Task<int> DeleteUserAsync(User user)
	{
		_db.Users.Remove(user);
		await this.SaveChangesAsync();
		return 1;
	}

	private async Task SaveChangesAsync()
	{
		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
		{
			throw new UsernameAlreadyExistsException();
		}
	}
}
