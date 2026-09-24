import { expect, test, type Page } from "@playwright/test";

test.use({ locale: "en-US", viewport: { width: 1280, height: 720 } });

// Screenshots for PR review (CI artifact `web-screenshots`)
const shot = (page: Page, name: string) => page.screenshot({ path: `screenshots/${name}.png`, fullPage: true });

test("full sale: create customer, sell, view invoice, stock goes down", async ({ page }) =>
{
	await page.goto("/");
	await expect(page.getByRole("cell", { name: "Screw" })).toBeVisible();
	await shot(page, "1-stock-items");

	await page.getByRole("button", { name: "Clients" }).click();
	await page.getByLabel("Name", { exact: true }).fill("Ana");
	await page.getByLabel("Lastname").fill("Gómez");
	await page.getByRole("button", { name: "Create customer" }).click();
	await expect(page.getByRole("cell", { name: "Gómez" })).toBeVisible();
	await shot(page, "2-customers");

	await page.getByRole("button", { name: "New sale" }).click();
	const customerId = await page.getByRole("option", { name: "Ana Gómez" }).getAttribute("value");
	await page.getByLabel("Customer").selectOption(customerId!);
	await page.getByRole("combobox", { name: /^Stock item/ }).selectOption("A1");
	await page.getByLabel("Quantity").fill("2");
	await page.getByRole("button", { name: "Add to shopping cart" }).click();
	await shot(page, "3-new-sale");
	await page.getByRole("button", { name: "Sell" }).click();

	await expect(page.getByRole("article")).toContainText("Screw");
	await expect(page.getByRole("article")).toContainText("Cash");
	await expect(page.getByTestId("invoice-total")).toHaveText("10,000");
	await shot(page, "4-invoice");

	await page.getByRole("button", { name: "Back" }).click();
	await expect(page.getByRole("cell", { name: "Ana Gómez" })).toBeVisible();
	await shot(page, "5-invoice-list");

	await page.getByRole("button", { name: "Stock items" }).click();
	await expect(page.getByRole("row", { name: /Screw A1/ })).toContainText("8");
});
