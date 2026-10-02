import { describe, expect, it } from "vitest";
import { mockApi } from "../test-utils";
import { searchApi } from "./search";

describe("searchApi", () =>
{
	it("search_Match_ReturnsGroups", async () =>
	{
		// Arrange
		const groups = [{ domain: "StockItems", items: [{ id: "1", title: "Cemento", subtitle: "C-1" }], totalCount: 1 }];
		mockApi({ "GET /api/search?q=cemento&includeInactive=false": { body: { groups } } });

		// Act
		const result = await searchApi.search("cemento", false);

		// Assert
		expect(result).toEqual({ ok: true, value: { groups } });
	});

	it("search_NetworkDown_ReturnsUnexpected", async () =>
	{
		// Arrange
		mockApi({});

		// Act
		const result = await searchApi.search("cemento", false);

		// Assert
		expect(result).toEqual({ ok: false, failure: { kind: "unexpected" } });
	});
});
