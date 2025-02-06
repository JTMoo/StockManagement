using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace StockManagement.Gui.ViewModel.Primary;


public class LoginViewModel : ViewModelBase
{
	private readonly IUserServiceProvider _userServiceProvider;


	private LoginViewModel(IUserServiceProvider userServiceProvider)
	{
		_userServiceProvider = userServiceProvider;
	}


	public ObservableCollection<User> AllUsers { get; private set; }


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
