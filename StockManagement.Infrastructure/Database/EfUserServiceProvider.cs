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


	public Task<User?> GetUserAsync(string id, CancellationToken cancellationToken = default)
	{
		return _db.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
	}

	public Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
	{
		return _db.Users.SingleOrDefaultAsync(user => user.Username == username, cancellationToken);
	}

	public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
	{
		return await _db.Users.ToListAsync(cancellationToken);
	}

	/// <exception cref="UsernameAlreadyExistsException">Username already in use</exception>
	public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
	{
		_db.Users.Add(user);
		await this.SaveChangesAsync(cancellationToken);
	}

	/// <exception cref="UsernameAlreadyExistsException">Username already in use</exception>
	public async Task<int> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
	{
		_db.Users.Update(user);
		await this.SaveChangesAsync(cancellationToken);
		return 1;
	}

	public async Task<int> DeleteUserAsync(User user, CancellationToken cancellationToken = default)
	{
		_db.Users.Remove(user);
		await this.SaveChangesAsync(cancellationToken);
		return 1;
	}

	private async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		try
		{
			await _db.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
		{
			throw new UsernameAlreadyExistsException();
		}
	}
}
