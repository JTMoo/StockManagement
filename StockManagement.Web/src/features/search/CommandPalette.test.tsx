import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Box } from "lucide-react";
import { describe, expect, it, vi } from "vitest";
import { mockApi, renderEnglish } from "../../test-utils";
import type { NavRoute } from "../../routes";
import { CommandPalette } from "./CommandPalette";

const routes: NavRoute[] = [{ name: "stockItems", icon: Box, render: () => null }];

describe("CommandPalette", () =>
{
	it("Typing_MatchesStockItem_ShowsHitAndCount", async () =>
	{
		// Arrange
		mockApi({
			"GET /api/search?q=cemento&includeInactive=false": {
				body: { groups: [{ domain: "StockItems", items: [{ id: "1", title: "Cemento Portland", subtitle: "C-4521" }], totalCount: 1 }] }
			}
		});
		renderEnglish(<CommandPalette routes={routes} onNavigate={vi.fn()} onClose={vi.fn()} />);

		// Act
		await userEvent.type(screen.getByLabelText("Search:"), "cemento");

		// Assert
		expect(await screen.findByRole("button", { name: /Cemento Portland/ })).toBeInTheDocument();
	});

	it("Typing_MatchesScreenName_ShowsScreensSection", async () =>
	{
		// Arrange
		mockApi({ "GET /api/search?q=stock&includeInactive=false": { body: { groups: [] } } });
		renderEnglish(<CommandPalette routes={routes} onNavigate={vi.fn()} onClose={vi.fn()} />);

		// Act
		await userEvent.type(screen.getByLabelText("Search:"), "stock");

		// Assert
		expect(await screen.findByText("Screens")).toBeInTheDocument();
	});

	it("ClickingScreen_NavigatesAndCloses", async () =>
	{
		// Arrange
		mockApi({ "GET /api/search?q=stock&includeInactive=false": { body: { groups: [] } } });
		const onNavigate = vi.fn();
		const onClose = vi.fn();
		renderEnglish(<CommandPalette routes={routes} onNavigate={onNavigate} onClose={onClose} />);
		await userEvent.type(screen.getByLabelText("Search:"), "stock");

		// Act
		await userEvent.click(await screen.findByRole("button", { name: "Stock items" }));

		// Assert
		expect(onNavigate).toHaveBeenCalledWith("stockItems");
		expect(onClose).toHaveBeenCalled();
	});

	it("Escape_ClosesPalette", async () =>
	{
		// Arrange
		mockApi({});
		const onClose = vi.fn();
		renderEnglish(<CommandPalette routes={routes} onNavigate={vi.fn()} onClose={onClose} />);

		// Act
		await userEvent.keyboard("{Escape}");

		// Assert
		expect(onClose).toHaveBeenCalled();
	});
});
