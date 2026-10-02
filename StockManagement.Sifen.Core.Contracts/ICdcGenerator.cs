namespace StockManagement.Sifen.Core.Contracts;


public interface ICdcGenerator
{
	/// <summary>
	/// Builds the 44-digit CDC for <paramref name="input"/>.
	/// </summary>
	string Generate(CdcInput input);
}
