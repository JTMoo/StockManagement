namespace StockManagement.Kernel.Model.Types;


public enum InvoiceStatus
{
	Open = 0,

	PartiallyPaid,

	Paid,

	Overdue,

	Cancelled
}
