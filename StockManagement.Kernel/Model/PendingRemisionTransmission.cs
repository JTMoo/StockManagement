using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


/// <summary>
/// Outbox row for one SIFEN transmission attempt of a <see cref="RemissionNote"/> (#162) - same shape and retry
/// semantics as <see cref="PendingTransmission"/>, kept separate because it points at a different document entity.
/// </summary>
public class PendingRemisionTransmission : BaseDocument
{
	private int attempts;
	private DateTime nextAttemptAt;
	private string lastError = "";


	public PendingRemisionTransmission()
	{
	}

	public PendingRemisionTransmission(RemissionNote remissionNote, DateTime nextAttemptAt)
	{
		this.RemissionNote = remissionNote;
		this.NextAttemptAt = nextAttemptAt;
	}


	public RemissionNote RemissionNote { get; set; } = null!;

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
