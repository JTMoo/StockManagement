import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { ImportPanel } from "./ImportPanel";

const file = new File(["dummy"], "legacy.xlsx", { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

const previewed = {
	id: "batch-1",
	target: "Customers",
	fileName: "legacy.xlsx",
	sheetName: "Sheet1",
	status: "Previewed",
	readyCount: 1,
	duplicateCount: 1,
	errorCount: 0,
	rows: [
		{ row: 2, status: "Ready", fields: { Name: "Ann" } },
		{ row: 3, status: "Duplicate", fields: { Name: "Bo" } }
	]
};

describe("ImportPanel", () =>
{
	it("Preview_Valid_SendsTheGivenTargetAndShowsCountsAndNonReadyRows", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "POST /api/import/batches": { body: previewed } });
		renderEnglish(<ImportPanel target="Customers" />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));

		// Assert
		expect(await screen.findByRole("cell", { name: "3" })).toBeInTheDocument();
		expect(screen.getByRole("cell", { name: "Bo" })).toBeInTheDocument();
		expect(screen.getAllByText("1")).toHaveLength(2);
		const [, init] = fetchMock.mock.calls.find(([url]) => url === "/api/import/batches")!;
		expect((init!.body as FormData).get("Target")).toBe("Customers");
	});

	it("PreviewThenCommit_ReadyRows_ShowsCommittedAndNotifies", async () =>
	{
		// Arrange
		mockApi({
			"POST /api/import/batches": { body: previewed },
			"POST /api/import/batches/batch-1/commit": { body: { ...previewed, status: "Committed" } }
		});
		const onImported = vi.fn();
		renderEnglish(<ImportPanel target="Customers" onImported={onImported} />);
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));
		await screen.findByRole("button", { name: "Commit" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Commit" }));

		// Assert
		expect(await screen.findByRole("button", { name: "Undo" })).toBeInTheDocument();
		expect(onImported).toHaveBeenCalledOnce();
	});

	it("PreviewCommitThenUndo_CommittedBatch_ShowsPreviewedAgainAndNotifies", async () =>
	{
		// Arrange
		mockApi({
			"POST /api/import/batches": { body: previewed },
			"POST /api/import/batches/batch-1/commit": { body: { ...previewed, status: "Committed" } },
			"POST /api/import/batches/batch-1/undo": { body: { ...previewed, status: "Undone" } }
		});
		const onImported = vi.fn();
		renderEnglish(<ImportPanel target="Customers" onImported={onImported} />);
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));
		await userEvent.click(await screen.findByRole("button", { name: "Commit" }));
		await screen.findByRole("button", { name: "Undo" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Undo" }));

		// Assert
		await screen.findByRole("cell", { name: "3" });
		expect(onImported).toHaveBeenCalledTimes(2);
	});

	it("NoFileChosen_PreviewButtonDisabled", () =>
	{
		// Arrange
		mockApi({});

		// Act
		renderEnglish(<ImportPanel target="Customers" />);

		// Assert
		expect(screen.getByRole("button", { name: "Preview" })).toBeDisabled();
	});

	it("Preview_ApiFails_ShowsError", async () =>
	{
		// Arrange
		mockApi({});
		renderEnglish(<ImportPanel target="Customers" />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));

		// Assert
		expect(await screen.findByRole("alert")).toHaveTextContent("An unexpected error occured");
	});
});
