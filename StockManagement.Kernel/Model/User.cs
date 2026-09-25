using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


public class User : BaseDocument
{
	private string username = string.Empty;
	private string passwordHash = string.Empty;


	public string Username
	{
		get { return this.username; }
		set { this.SetField(ref this.username, value); }
	}

	/// <summary>
	/// PBKDF2-SHA256 hash, never the plain password
	/// </summary>
	public string PasswordHash
	{
		get { return this.passwordHash; }
		set { this.SetField(ref this.passwordHash, value); }
	}
}
