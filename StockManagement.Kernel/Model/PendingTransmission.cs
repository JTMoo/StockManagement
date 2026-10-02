using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


/// <summary>
/// Outbox row for one SIFEN transmission attempt of an <see cref="Invoice"/> (ADR-0031). Removed once the invoice
/// reaches <see cref="Model.Types.TransmissionStatus.Accepted"/> or <see cref="Model.Types.TransmissionStatus.Rejected"/>;
/// rescheduled on <see cref="Model.Types.TransmissionStatus.Error"/> until the 72h deadline passes.
/// </summary>
public class PendingTransmission : BaseDocument
{
	private int attempts;
	private DateTime nextAttemptAt;
	private string lastError = "";


	public PendingTransmission()
	{
	}

	public PendingTransmission(Invoice invoice, DateTime nextAttemptAt)
	{
		this.Invoice = invoice;
		this.NextAttemptAt = nextAttemptAt;
	}


	public Invoice Invoice { get; set; } = null!;

	public int Attempts
	{
		get { return this.attempts; }
		set { this.SetField(ref this.attempts, value); }
	}

	public DateTime NextAttemptAt
	{
		get { return this.nextAttemptAt; }
		set { this.SetField(ref this.nextAttemptAt, value); }
	}

	public string LastError
	{
		get { return this.lastError; }
		set { this.SetField(ref this.lastError, value); }
	}
}
