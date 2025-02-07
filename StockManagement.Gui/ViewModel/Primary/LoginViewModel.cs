using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StockManagement.Gui.ViewModel.Primary;


public class LoginViewModel : ViewModelBase
{
	private readonly IUserServiceProvider _userServiceProvider;
	private string _username = string.Empty;
	private string _password = string.Empty;


	private LoginViewModel(IUserServiceProvider userServiceProvider)
	{
		_userServiceProvider = userServiceProvider;
	}


	#region Properties
	public ObservableCollection<User> AllUsers { get; private set; }

	public string Username
	{
		get { return _username; }
		set { this.SetField(ref _username, value); }
	}

	public string Password
	{
		get { return _password; }
		set { this.SetField(ref _password, value); }
	}
	#region Properties


	public static Task<LoginViewModel> CreateAsync(IUserServiceProvider userServiceProvider)
	{
		var ret = new LoginViewModel(userServiceProvider);
		return ret.InitializeAsync();
	}

	private async Task<LoginViewModel> InitializeAsync()
	{
		this.AllUsers = new(await _userServiceProvider.GetAllUsersAsync());
		return this;
	}
}
