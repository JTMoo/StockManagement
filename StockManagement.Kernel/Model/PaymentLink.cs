using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


/// <summary>
/// A Bancard QR/payment-link generated for an invoice (#150). <see cref="Kernel.Model.Invoice.Payments"/> is still
/// the only source of truth for amount paid; a link only exists to track the external Bancard transaction until
/// it is reconciled into a <see cref="Payment"/>.
/// </summary>
public class PaymentLink : BaseDocument
{
	private string externalId = "";
	private string qrUrl = "";
	private decimal amount;
	private PaymentLinkStatus status = PaymentLinkStatus.Pending;
	private DateTime createdAt;
	private DateTime expiresAt;
	private DateTime? paidAt;


	public Invoice Invoice { get; set; } = null!;

	/// <summary>Bancard's <c>shop_process_id</c></summary>
	public string ExternalId
	{
		get { return this.externalId; }
		set { this.SetField(ref this.externalId, value); }
	}

	public string QrUrl
	{
		get { return this.qrUrl; }
		set { this.SetField(ref this.qrUrl, value); }
	}

	public decimal Amount
	{
		get { return this.amount; }
		set { this.SetField(ref this.amount, value); }
	}

	public PaymentLinkStatus Status
	{
		get { return this.status; }
		set { this.SetField(ref this.status, value); }
	}

	public DateTime CreatedAt
	{
		get { return this.createdAt; }
		set { this.SetField(ref this.createdAt, value); }
	}

	public DateTime ExpiresAt
	{
		get { return this.expiresAt; }
		set { this.SetField(ref this.expiresAt, value); }
	}

	public DateTime? PaidAt
	{
		get { return this.paidAt; }
		set { this.SetField(ref this.paidAt, value); }
	}
}
