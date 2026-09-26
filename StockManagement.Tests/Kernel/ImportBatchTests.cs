using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class ImportBatchTests
{
	[TestMethod]
	public void Ctor_Rows_IsPreviewedWithReadyRowsSeparated()
	{
		// Arrange
		ImportBatchRow[] rows =
		[
			new(1, ImportRowStatus.Ready, null, "{}"),
			new(2, ImportRowStatus.Duplicate, null, "{}"),
			new(3, ImportRowStatus.Error, "bad", null)
		];

		// Act
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", rows);

		// Assert
		Assert.AreEqual(ImportBatchStatus.Previewed, batch.Status);
		Assert.AreEqual(3, batch.Rows.Count);
		CollectionAssert.AreEqual(new[] { 1 }, batch.ReadyRows.Select(row => row.RowNumber).ToList());
	}

	[TestMethod]
	public void MarkCommitted_OneIdPerReadyRow_AssignsThemAndAdvancesStatus()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1",
			[new ImportBatchRow(1, ImportRowStatus.Ready, null, "{}"), new ImportBatchRow(2, ImportRowStatus.Duplicate, null, "{}")]);

		// Act
		batch.MarkCommitted(["new-id"]);

		// Assert
		Assert.AreEqual(ImportBatchStatus.Committed, batch.Status);
		Assert.IsNotNull(batch.CommittedAtUtc);
		Assert.AreEqual("new-id", batch.ReadyRows[0].ImportedEntityId);
	}

	[TestMethod]
	public void MarkCommitted_AlreadyCommitted_Throws()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", [new ImportBatchRow(1, ImportRowStatus.Ready, null, "{}")]);
		batch.MarkCommitted(["new-id"]);

		// Act & Assert
		Assert.ThrowsException<InvalidImportBatchStatusException>(() => batch.MarkCommitted(["other-id"]));
	}

	[TestMethod]
	public void MarkCommitted_WrongNumberOfIds_Throws()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", [new ImportBatchRow(1, ImportRowStatus.Ready, null, "{}")]);

		// Act & Assert
		Assert.ThrowsException<ArgumentException>(() => batch.MarkCommitted([]));
	}

	[TestMethod]
	public void MarkUndone_Committed_ClearsImportedIdsAndAdvancesStatus()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", [new ImportBatchRow(1, ImportRowStatus.Ready, null, "{}")]);
		batch.MarkCommitted(["new-id"]);

		// Act
		batch.MarkUndone();

		// Assert
		Assert.AreEqual(ImportBatchStatus.Undone, batch.Status);
		Assert.IsNotNull(batch.UndoneAtUtc);
		Assert.IsNull(batch.Rows[0].ImportedEntityId);
	}

	[TestMethod]
	public void MarkUndone_NotCommittedYet_Throws()
	{
		// Arrange
		var batch = new ImportBatch(ImportTarget.Customers, "legacy.xlsx", "Sheet1", [new ImportBatchRow(1, ImportRowStatus.Ready, null, "{}")]);

		// Act & Assert
		Assert.ThrowsException<InvalidImportBatchStatusException>(batch.MarkUndone);
	}
}
