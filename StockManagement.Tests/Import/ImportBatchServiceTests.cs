using Moq;
using StockManagement.Import.Core;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class ImportBatchServiceTests
{
	private readonly Mock<IImportTargetHandler> _handler = new();
	private readonly Mock<IImportBatchServiceProvider> _batches = new();
	private readonly ImportBatchService _service;


	public ImportBatchServiceTests()
	{
		_handler.SetupGet(handler => handler.Target).Returns(ImportTarget.Customers);
		_handler.Setup(handler => handler.SerializeCandidate(It.IsAny<object>())).Returns((object candidate) => $"payload:{candidate}");
		_service = new ImportBatchService([_handler.Object], _batches.Object);
	}


	[TestMethod]
	public async Task PreviewAsync_ParsedRows_SplitsReadyDuplicateAndErrorRows()
	{
		// Arrange
		object fresh = "fresh";
		object clash = "clash";
		_handler.Setup(handler => handler.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(("Sheet1", (IReadOnlyList<(int Row, object Candidate)>)[(2, fresh), (3, clash)], (IReadOnlyList<ImportRowError>)[new ImportRowError(4, "bad")]));
		_handler.Setup(handler => handler.SplitDuplicatesAsync(It.IsAny<IReadOnlyList<object>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new DuplicateFilterResult<object>([fresh], [clash]));

		ImportBatch? added = null;
		_batches.Setup(provider => provider.AddImportBatchAsync(It.IsAny<ImportBatch>())).Callback<ImportBatch>(batch => added = batch).Returns(Task.CompletedTask);

		// Act
		var batch = await _service.PreviewAsync(ImportTarget.Customers, "legacy.xlsx", Stream.Null);

		// Assert
		Assert.AreSame(added, batch);
		Assert.AreEqual(ImportBatchStatus.Previewed, batch.Status);
		Assert.AreEqual("Sheet1", batch.SheetName);
		CollectionAssert.AreEqual(new[] { 2, 3, 4 }, batch.Rows.Select(row => row.RowNumber).ToList());
		Assert.AreEqual(ImportRowStatus.Ready, batch.Rows.Single(row => row.RowNumber == 2).Status);
		Assert.AreEqual(ImportRowStatus.Duplicate, batch.Rows.Single(row => row.RowNumber == 3).Status);
		Assert.AreEqual(ImportRowStatus.Error, batch.Rows.Single(row => row.RowNumber == 4).Status);
		Assert.AreEqual("bad", batch.Rows.Single(row => row.RowNumber == 4).ErrorMessage);
	}

	[TestMethod]
	public async Task CommitAsync_UnknownBatch_ThrowsNotFound()
	{
		// Arrange
		_batches.Setup(provider => provider.GetImportBatchAsync("missing")).ReturnsAsync((ImportBatch?)null);

		// Act & Assert
		await Assert.ThrowsExceptionAsync<ImportBatchNotFoundException>(() => _service.CommitAsync("missing"));
	}

	[TestMethod]
	public async Task CommitAsync_PreviewedBatch_DeserializesReadyRowsAndMarksCommitted()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1",
			[new ImportBatchRow(1, ImportRowStatus.Ready, null, "payload:fresh"), new ImportBatchRow(2, ImportRowStatus.Duplicate, null, "payload:clash")]);
		_batches.Setup(provider => provider.GetImportBatchAsync(batch.Id)).ReturnsAsync(batch);
		_handler.Setup(handler => handler.DeserializeCandidate("payload:fresh")).Returns("fresh");
		_handler.Setup(handler => handler.CommitAsync(It.Is<IReadOnlyList<object>>(list => list.SequenceEqual(new object[] { "fresh" })), It.IsAny<CancellationToken>()))
			.ReturnsAsync((IReadOnlyList<string>)["new-id"]);

		// Act
		var result = await _service.CommitAsync(batch.Id);

		// Assert
		Assert.AreEqual(ImportBatchStatus.Committed, result.Status);
		Assert.AreEqual("new-id", result.ReadyRows[0].ImportedEntityId);
		_batches.Verify(provider => provider.UpdateImportBatchAsync(batch), Times.Once);
	}

	[TestMethod]
	public async Task UndoAsync_CommittedBatch_RemovesImportedEntitiesAndMarksUndone()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", [new ImportBatchRow(1, ImportRowStatus.Ready, null, "payload:fresh")]);
		batch.MarkCommitted(["new-id"]);
		_batches.Setup(provider => provider.GetImportBatchAsync(batch.Id)).ReturnsAsync(batch);

		// Act
		var result = await _service.UndoAsync(batch.Id);

		// Assert
		Assert.AreEqual(ImportBatchStatus.Undone, result.Status);
		_handler.Verify(handler => handler.UndoAsync(It.Is<IReadOnlyList<string>>(ids => ids.SequenceEqual(new[] { "new-id" })), It.IsAny<CancellationToken>()), Times.Once);
	}

	[TestMethod]
	public async Task UndoAsync_NotCommittedYet_ThrowsInvalidStatus()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", [new ImportBatchRow(1, ImportRowStatus.Ready, null, "payload:fresh")]);
		_batches.Setup(provider => provider.GetImportBatchAsync(batch.Id)).ReturnsAsync(batch);

		// Act & Assert
		await Assert.ThrowsExceptionAsync<InvalidImportBatchStatusException>(() => _service.UndoAsync(batch.Id));
	}
}
