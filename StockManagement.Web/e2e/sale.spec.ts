import { expect, test } from "@playwright/test";

test.use({ locale: "en-US" });

test("full sale: create customer, sell, view invoice, stock goes down", async ({ page }) =>
{
	await page.goto("/");
	await expect(page.getByRole("cell", { name: "Screw" })).toBeVisible();

	await page.getByRole("button", { name: "Clients" }).click();
	await page.getByLabel("Name", { exact: true }).fill("Ana");
	await page.getByLabel("Lastname").fill("Gómez");
	await page.getByRole("button", { name: "Create customer" }).click();
	await expect(page.getByRole("cell", { name: "Gómez" })).toBeVisible();

	await page.getByRole("button", { name: "New sale" }).click();
	const customerId = await page.getByRole("option", { name: "Ana Gómez" }).getAttribute("value");
	await page.getByLabel("Customer").selectOption(customerId!);
	await page.getByLabel("Stock item").selectOption("A1");
	await page.getByLabel("Quantity").fill("2");
	await page.getByRole("button", { name: "Add to shopping cart" }).click();
	await page.getByRole("button", { name: "Sell" }).click();

	await expect(page.getByRole("article")).toContainText("Screw");
	await expect(page.getByTestId("invoice-total")).toHaveText("10,000");

	await page.getByRole("button", { name: "Stock items" }).click();
	await expect(page.getByRole("row", { name: /A1 Screw/ })).toContainText("8");
});
