namespace StockManagement.Sifen.Core;


/// <summary>
/// Exponential backoff for <see cref="Contracts.SifenTransmissionOutcome.Error"/>, capped and bounded by DNIT's 72h
/// transmission deadline (ADR-0031). <see cref="Contracts.SifenTransmissionOutcome.Rejected"/> never retries - callers
/// don't consult this policy for it.
/// </summary>
public static class TransmissionRetryPolicy
{
	private static readonly TimeSpan Deadline = TimeSpan.FromHours(72);
	private static readonly TimeSpan BaseDelay = TimeSpan.FromMinutes(1);
	private static readonly TimeSpan MaxDelay = TimeSpan.FromHours(1);


	/// <returns>
	/// When the next attempt should run; <see langword="null"/> once <paramref name="now"/> is at or past
	/// <paramref name="invoiceDate"/> + 72h - the deadline has passed and the caller should stop retrying.
	/// </returns>
	public static DateTime? NextAttempt(DateTime invoiceDate, int attempts, DateTime now)
	{
		var deadline = invoiceDate + Deadline;
		if (now >= deadline) return null;

		var delayMs = Math.Min(BaseDelay.TotalMilliseconds * Math.Pow(2, Math.Max(attempts, 0)), MaxDelay.TotalMilliseconds);
		var next = now + TimeSpan.FromMilliseconds(delayMs);
		return next < deadline ? next : deadline;
	}
}
