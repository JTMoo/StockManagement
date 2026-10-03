using Moq;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sifen.Core;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Tests.Sifen;


[TestClass]
public sealed class SifenTransmissionProcessorTests
{
	private readonly Mock<IPendingTransmissionServiceProvider> _pendingTransmissions = new();

	[TestMethod]
	public async Task ApplyAsync_Accepted_MarksInvoiceAcceptedWithCdc()
	{
		// Arrange
		var transmission = new PendingTransmission(new Invoice(), DateTime.Now);
		var result = SifenTransmissionResult.Accepted("01" + new string('0', 42));

		// Act
		await SifenTransmissionProcessor.ApplyAsync(_pendingTransmissions.Object, transmission, result, DateTime.Now);

		// Assert
		_pendingTransmissions.Verify(provider => provider.MarkTerminalAsync(transmission, TransmissionStatus.Accepted, result.Cdc!, It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task ApplyAsync_Rejected_MarksInvoiceRejectedAndDoesNotReschedule()
	{
		// Arrange
		var transmission = new PendingTransmission(new Invoice(), DateTime.Now);
		var result = SifenTransmissionResult.Rejected("cdc", "invalid RUC");

		// Act
		await SifenTransmissionProcessor.ApplyAsync(_pendingTransmissions.Object, transmission, result, DateTime.Now);

		// Assert
		_pendingTransmissions.Verify(provider => provider.MarkTerminalAsync(transmission, TransmissionStatus.Rejected, "cdc", It.IsAny<CancellationToken>()), Times.Once);
		_pendingTransmissions.Verify(provider => provider.MarkErrorAsync(It.IsAny<PendingTransmission>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[TestMethod]
	public async Task ApplyAsync_Error_SchedulesNextAttemptViaRetryPolicy()
	{
		// Arrange
		var now = new DateTime(2026, 10, 2, 12, 0, 0);
		var invoice = new Invoice { Date = now };
		var transmission = new PendingTransmission(invoice, now) { Attempts = 0 };
		var result = SifenTransmissionResult.Error("timeout");

		// Act
		await SifenTransmissionProcessor.ApplyAsync(_pendingTransmissions.Object, transmission, result, now);

		// Assert
		_pendingTransmissions.Verify(provider => provider.MarkErrorAsync(transmission, "timeout", now.AddMinutes(1), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task ApplyAsync_ErrorPastDeadline_SchedulesNoFurtherAttempt()
	{
		// Arrange
		var invoiceDate = new DateTime(2026, 10, 2, 12, 0, 0);
		var now = invoiceDate.AddHours(73);
		var invoice = new Invoice { Date = invoiceDate };
		var transmission = new PendingTransmission(invoice, now) { Attempts = 5 };
		var result = SifenTransmissionResult.Error("timeout");

		// Act
		await SifenTransmissionProcessor.ApplyAsync(_pendingTransmissions.Object, transmission, result, now);

		// Assert
		_pendingTransmissions.Verify(provider => provider.MarkErrorAsync(transmission, "timeout", null, It.IsAny<CancellationToken>()), Times.Once);
	}
}


/// <summary>
/// Same decisions as <see cref="SifenTransmissionProcessorTests"/>, for <see cref="SifenTransmissionProcessor.ApplyRemisionAsync"/> (#162).
/// </summary>
[TestClass]
public sealed class SifenTransmissionProcessorRemisionTests
{
	private readonly Mock<IPendingRemisionTransmissionServiceProvider> _pendingTransmissions = new();

	[TestMethod]
	public async Task ApplyRemisionAsync_Accepted_MarksRemissionNoteAcceptedWithCdc()
	{
		// Arrange
		var transmission = new PendingRemisionTransmission(new RemissionNote(), DateTime.Now);
		var result = SifenTransmissionResult.Accepted("07" + new string('0', 42));

		// Act
		await SifenTransmissionProcessor.ApplyRemisionAsync(_pendingTransmissions.Object, transmission, result, DateTime.Now);

		// Assert
		_pendingTransmissions.Verify(provider => provider.MarkTerminalAsync(transmission, TransmissionStatus.Accepted, result.Cdc!, It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task ApplyRemisionAsync_Error_SchedulesNextAttemptViaRetryPolicy()
	{
		// Arrange
		var now = new DateTime(2026, 10, 3, 12, 0, 0);
		var remissionNote = new RemissionNote { Date = now };
		var transmission = new PendingRemisionTransmission(remissionNote, now) { Attempts = 0 };
		var result = SifenTransmissionResult.Error("timeout");

		// Act
		await SifenTransmissionProcessor.ApplyRemisionAsync(_pendingTransmissions.Object, transmission, result, now);

		// Assert
		_pendingTransmissions.Verify(provider => provider.MarkErrorAsync(transmission, "timeout", now.AddMinutes(1), It.IsAny<CancellationToken>()), Times.Once);
	}
}
