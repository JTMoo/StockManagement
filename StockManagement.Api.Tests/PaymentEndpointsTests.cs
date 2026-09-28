using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.Invoices;
using StockManagement.Api.Features.Payments;
using StockManagement.Api.Features.Sales;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class PaymentEndpointsTests
{
	private ApiFactory _factory;
	private HttpClient _client;
	private Customer _customer;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = await _factory.CreateAuthenticatedClientAsync();

		await _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>().AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10, price: 5000));
		_customer = new Customer() { CustomerId = 1001, Name = "Ana" };
		await _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>().AddCustomerAsync(_customer);
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task CreatePayment_PartialAmount_Returns201AndInvoiceIsPartiallyPaid()
	{
		// Arrange
		var invoiceNumber = await this.CreateCreditSaleAsync();

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/payments", new CreatePaymentRequest(invoiceNumber, 5000, PaymentMethod.Cash, null), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var payment = await response.Content.ReadAsAsync<PaymentResponse>();
		Assert.AreEqual(5000, payment.Amount);
		Assert.AreEqual(PaymentMethod.Cash, payment.Method);

		var invoice = await _client.GetFromJsonAsync<InvoiceResponse>($"/api/invoices/{invoiceNumber}", ApiFactory.JsonOptions);
		Assert.AreEqual(5000, invoice.AmountPaid);
		Assert.AreEqual(10000, invoice.AmountDue);
		Assert.AreEqual(InvoiceStatus.PartiallyPaid, invoice.Status);
	}

	[TestMethod]
	public async Task CreatePayment_CoversFullAmountDue_InvoiceBecomesPaid()
	{
		// Arrange
		var invoiceNumber = await this.CreateCreditSaleAsync();

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/payments", new CreatePaymentRequest(invoiceNumber, 15000, PaymentMethod.BankTransfer, null), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var invoice = await _client.GetFromJsonAsync<InvoiceResponse>($"/api/invoices/{invoiceNumber}", ApiFactory.JsonOptions);
		Assert.AreEqual(0, invoice.AmountDue);
		Assert.AreEqual(InvoiceStatus.Paid, invoice.Status);
	}

	[TestMethod]
	public async Task CreatePayment_ExceedsAmountDue_Returns409AndWritesNothing()
	{
		// Arrange
		var invoiceNumber = await this.CreateCreditSaleAsync();

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/payments", new CreatePaymentRequest(invoiceNumber, 20000, PaymentMethod.Cash, null), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
		var conflict = await response.Content.ReadAsAsync<PaymentRejectedResponse>();
		Assert.AreEqual("paymentExceedsAmountDue", conflict.Reason);
		var invoice = await _client.GetFromJsonAsync<InvoiceResponse>($"/api/invoices/{invoiceNumber}", ApiFactory.JsonOptions);
		Assert.AreEqual(0, invoice.AmountPaid);
	}

	[TestMethod]
	public async Task CreatePayment_ZeroAmount_Returns400()
	{
		// Arrange
		var invoiceNumber = await this.CreateCreditSaleAsync();

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/payments", new CreatePaymentRequest(invoiceNumber, 0, PaymentMethod.Cash, null), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task CreatePayment_UnknownInvoice_Returns404()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/invoices/999/payments", new CreatePaymentRequest(999, 100, PaymentMethod.Cash, null), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task ListPayments_TwoPayments_ReturnsBothOldestFirst()
	{
		// Arrange
		var invoiceNumber = await this.CreateCreditSaleAsync();
		await _client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/payments", new CreatePaymentRequest(invoiceNumber, 4000, PaymentMethod.Cash, new DateTime(2026, 9, 2)), ApiFactory.JsonOptions);
		await _client.PostAsJsonAsync($"/api/invoices/{invoiceNumber}/payments", new CreatePaymentRequest(invoiceNumber, 3000, PaymentMethod.Check, new DateTime(2026, 9, 1)), ApiFactory.JsonOptions);

		// Act
		var response = await _client.GetFromJsonAsync<InvoicePaymentsResponse>($"/api/invoices/{invoiceNumber}/payments", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(2, response.Items.Count);
		Assert.AreEqual(3000, response.Items[0].Amount);
		Assert.AreEqual(4000, response.Items[1].Amount);
		Assert.AreEqual(7000, response.AmountPaid);
		Assert.AreEqual(8000, response.AmountDue);
	}

	[TestMethod]
	public async Task ListOpenInvoices_ExcludesFullyPaidInvoices()
	{
		// Arrange
		var openNumber = await this.CreateCreditSaleAsync();
		var paidNumber = await this.CreateCreditSaleAsync();
		await _client.PostAsJsonAsync($"/api/invoices/{paidNumber}/payments", new CreatePaymentRequest(paidNumber, 15000, PaymentMethod.Cash, null), ApiFactory.JsonOptions);

		// Act
		var result = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices/open", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(1, result.TotalCount);
		Assert.AreEqual(openNumber, result.Items.Single().Number);
	}

	[TestMethod]
	public async Task ListOverdueInvoices_OnlyPastDueUnpaidInvoices()
	{
		// Arrange
		var invoices = _factory.ScopedServices.GetRequiredService<IInvoiceServiceProvider>();
		await invoices.AddInvoiceAsync(new Invoice() { Number = 1, Customer = _customer, Total = 1000, ExpirationDate = DateTime.Now.AddDays(-1), Items = [] });
		await invoices.AddInvoiceAsync(new Invoice() { Number = 2, Customer = _customer, Total = 1000, ExpirationDate = DateTime.Now.AddDays(10), Items = [] });

		// Act
		var result = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices/overdue", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(1, result.TotalCount);
		Assert.AreEqual(1, result.Items.Single().Number);
		Assert.AreEqual(InvoiceStatus.Overdue, result.Items.Single().Status);
	}

	/// <returns>Number of a stored 15000-total credit invoice for <see cref="_customer"/></returns>
	private async Task<int> CreateCreditSaleAsync()
	{
		var response = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(_customer.CustomerId, SaleCondition.Credit, [new("A1", 3)]), ApiFactory.JsonOptions);
		var invoice = await response.Content.ReadAsAsync<InvoiceResponse>();
		return invoice.Number;
	}
}
