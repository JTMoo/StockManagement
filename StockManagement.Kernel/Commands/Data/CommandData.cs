using System.Diagnostics;

namespace StockManagement.Kernel.Commands.Data;


public class CommandData
{
	public Action<bool> Callback { get; set; }

	public object Value { get; set; }

	public void InvokeCallback(bool success)
	{
		try
		{
			Callback?.Invoke(success);
		}
		catch (Exception ex)
		{
			Trace.WriteLine(ex.Message);
		}
	}
}