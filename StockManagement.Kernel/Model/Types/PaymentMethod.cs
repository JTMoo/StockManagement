namespace StockManagement.Kernel.Model.Types;


public enum PaymentMethod
{
	None = 0,

	Cash,

	BankTransfer,

	Check,

	/// <summary>Reconciled from a paid <see cref="PaymentLink"/> (#150)</summary>
	BancardQr,

	Other
}
