namespace StockManagement.Settings.Core.Contracts;


/// <summary>
/// Company-wide settings used across sales and imports
/// </summary>
/// <param name="CompanyName">Legal name shown on invoices</param>
/// <param name="TaxId">Tax identification number shown on invoices</param>
/// <param name="Currency">ISO 4217 currency code (e.g. "PYG"); display only for now</param>
/// <param name="VatRatePercent">VAT rate as a percentage (e.g. 10 for 10 %), included in stock item prices</param>
/// <param name="PaymentTermInDays">Days from an invoice's date to its due date</param>
/// <param name="FirstInvoiceNumber">Seed for the next invoice number when no invoice exists yet</param>
/// <param name="FirstCustomerId">Seed for the next customer id when no customer exists yet</param>
public sealed record CompanySettings(string CompanyName, string TaxId, string Currency, decimal VatRatePercent, int PaymentTermInDays, int FirstInvoiceNumber, int FirstCustomerId);
