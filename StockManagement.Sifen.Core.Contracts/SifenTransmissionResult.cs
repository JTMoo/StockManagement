namespace StockManagement.Sifen.Core.Contracts;


/// <param name="Cdc">Set on <see cref="SifenTransmissionOutcome.Accepted"/> and <see cref="SifenTransmissionOutcome.Rejected"/> (the DE was built and signed either way); null on <see cref="SifenTransmissionOutcome.Error"/></param>
/// <param name="Message">DNIT's response text, or the exception message on <see cref="SifenTransmissionOutcome.Error"/></param>
public sealed record SifenTransmissionResult(SifenTransmissionOutcome Outcome, string? Cdc, string Message)
{
	public static SifenTransmissionResult Accepted(string cdc) => new(SifenTransmissionOutcome.Accepted, cdc, "Autorizado");

	public static SifenTransmissionResult Rejected(string cdc, string message) => new(SifenTransmissionOutcome.Rejected, cdc, message);

	public static SifenTransmissionResult Error(string message) => new(SifenTransmissionOutcome.Error, null, message);
}
