import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import { ImportPanel } from "./ImportPanel";

const file = new File(["dummy"], "legacy.xlsx", { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });

const detected = {
	sheetName: "Sheet1",
	columns: [{ column: 1, header: "Name", matchedFieldName: "Name" }],
	fields: [{ name: "Name", displayName: "Name" }]
};

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

/** Uploads the file, detects columns, and confirms the (default) mapping — the shared setup before every preview-dependent assertion. */
async function uploadAndConfirmMapping()
{
	await userEvent.upload(screen.getByLabelText("Choose file"), file);
	await userEvent.click(screen.getByRole("button", { name: "Preview" }));
	await screen.findByLabelText("Name");
	await userEvent.click(screen.getByRole("button", { name: "Preview" }));
}

describe("ImportPanel", () =>
{
	it("DetectColumns_Valid_ShowsAMappingRowPerColumnDefaultedToTheAutoMatch", async () =>
	{
		// Arrange
		const fetchMock = mockApi({ "POST /api/import/batches/columns": { body: detected } });
		renderEnglish(<ImportPanel target="Customers" />);

		// Act
		await userEvent.upload(screen.getByLabelText("Choose file"), file);
		await userEvent.click(screen.getByRole("button", { name: "Preview" }));

		// Assert
		expect(await screen.findByLabelText("Name")).toHaveValue("Name");
		const [, init] = fetchMock.mock.calls.find(([url]) => url === "/api/import/batches/columns")!;
		expect((init!.body as FormData).get("Target")).toBe("Customers");
	});

	it("PreviewValid_SendsTheConfirmedMappingAndShowsCountsAndNonReadyRows", async () =>
	{
		// Arrange
		const fetchMock = mockApi({
			"POST /api/import/batches/columns": { body: detected },
			"POST /api/import/batches": { body: previewed }
		});
		renderEnglish(<ImportPanel target="Customers" />);

		// Act
		await uploadAndConfirmMapping();

		// Assert
		expect(await screen.findByRole("cell", { name: "3" })).toBeInTheDocument();
		expect(screen.getByRole("cell", { name: "Bo" })).toBeInTheDocument();
		expect(screen.getAllByText("1")).toHaveLength(2);
		const [, init] = fetchMock.mock.calls.find(([url]) => url === "/api/import/batches")!;
		const body = init!.body as FormData;
		expect(body.get("Target")).toBe("Customers");
		expect(JSON.parse(body.get("Mapping") as string)).toEqual({ 1: "Name" });
	});

	it("PreviewThenCommit_ReadyRows_ShowsCommittedAndNotifies", async () =>
	{
		// Arrange
		mockApi({
			"POST /api/import/batches/columns": { body: detected },
			"POST /api/import/batches": { body: previewed },
			"POST /api/import/batches/batch-1/commit": { body: { ...previewed, status: "Committed" } }
		});
		const onImported = vi.fn();
		renderEnglish(<ImportPanel target="Customers" onImported={onImported} />);
		await uploadAndConfirmMapping();
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
			"POST /api/import/batches/columns": { body: detected },
			"POST /api/import/batches": { body: previewed },
			"POST /api/import/batches/batch-1/commit": { body: { ...previewed, status: "Committed" } },
			"POST /api/import/batches/batch-1/undo": { body: { ...previewed, status: "Undone" } }
		});
		const onImported = vi.fn();
		renderEnglish(<ImportPanel target="Customers" onImported={onImported} />);
		await uploadAndConfirmMapping();
		await userEvent.click(await screen.findByRole("button", { name: "Commit" }));
		await screen.findByRole("button", { name: "Undo" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Undo" }));

		// Assert
		await screen.findByRole("cell", { name: "3" });
		expect(onImported).toHaveBeenCalledTimes(2);
	});

	it("PreviewWithErrorRows_DownloadReportClicked_FetchesTheReportAndTriggersADownload", async () =>
	{
		// Arrange
		const csv = "Row,Status,Message,Data\r\n3,Duplicate,,Name=Bo\r\n";
		mockApi({
			"POST /api/import/batches/columns": { body: detected },
			"POST /api/import/batches": { body: previewed },
			"GET /api/import/batches/batch-1/report": { body: csv }
		});
		vi.stubGlobal("URL", { ...URL, createObjectURL: vi.fn(() => "blob:mock"), revokeObjectURL: vi.fn() });
		renderEnglish(<ImportPanel target="Customers" />);
		await uploadAndConfirmMapping();
		await screen.findByRole("button", { name: "Download report" });

		// Act
		await userEvent.click(screen.getByRole("button", { name: "Download report" }));

		// Assert
		expect(URL.createObjectURL).toHaveBeenCalled();
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

	it("DetectColumns_ApiFails_ShowsError", async () =>
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
