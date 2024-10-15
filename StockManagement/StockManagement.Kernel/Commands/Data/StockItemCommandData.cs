namespace StockManagement.Kernel.Commands.Data;


public class StockItemCommandData : CommandData
{
	public object DataToRegister { get; set; }

	public CreationCommandType Type { get; set; }

	public enum CreationCommandType
	{
		None,

		Single,

		Multiple
	}
}