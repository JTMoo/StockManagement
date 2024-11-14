using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;

namespace StockManagement.Kernel.Commands;


public class ChangeSettingsCommand : ICommand
{
	public CommandData Data { get; set; }


	public async Task<bool> Execute()
	{
		if (Data.Value is not Settings value) return false;
		if (CommonHelper.DeepClone(value) is not Settings settings) return false;

		await MainManager.Instance.Settings.Update(settings);
		return true;
	}
}