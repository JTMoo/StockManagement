namespace StockManagement.Kernel.Model.Types;


public enum PaymentLinkStatus
{
	Pending = 0,

	Paid,

	Expired,

	Cancelled,

	Failed
}
