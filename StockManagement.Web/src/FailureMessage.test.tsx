import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FailureMessage } from "./FailureMessage";
import { renderEnglish } from "./test-utils";

describe("FailureMessage", () =>
{
	it("Render_KnownCode_ShowsLocalizedText", () =>
	{
		// Arrange / Act
		renderEnglish(<FailureMessage failure={{ kind: "invalid", codes: ["customerNotFound"] }} />);

		// Assert
		expect(screen.getByRole("alert")).toHaveTextContent("Customer not found.");
	});

	it("Render_ServerText_ShowsGenericText", () =>
	{
		// Arrange / Act
		renderEnglish(<FailureMessage failure={{ kind: "invalid", codes: ["'name' must not be empty."] }} />);

		// Assert
		expect(screen.getByRole("alert")).toHaveTextContent("Please check your input.");
	});

	it("Render_Conflict_ListsUnavailableItems", () =>
	{
		// Arrange / Act
		renderEnglish(<FailureMessage failure={{ kind: "conflict", unavailableItems: ["Screw", "Nut"] }} />);

		// Assert
		expect(screen.getByRole("alert")).toHaveTextContent("Not enough stock: Screw, Nut");
	});

	it("Render_NoFailure_RendersNothing", () =>
	{
		// Arrange / Act
		renderEnglish(<FailureMessage />);

		// Assert
		expect(screen.queryByRole("alert")).not.toBeInTheDocument();
	});
});
