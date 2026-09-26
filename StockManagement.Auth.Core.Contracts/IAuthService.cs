using StockManagement.Kernel.Model;

namespace StockManagement.Auth.Core.Contracts;


public interface IAuthService
{
	/// <returns>The matching user, or <see langword="null"/> if the username or password is wrong</returns>
	public Task<User?> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);

	/// <summary>Hashes a password for storage on <see cref="User.PasswordHash"/>; never store a plain password.</summary>
	public string HashPassword(string password);
}
