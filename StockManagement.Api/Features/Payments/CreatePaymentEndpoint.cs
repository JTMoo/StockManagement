using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Payments;


public sealed record CreatePaymentRequest(int Number, decimal Amount, PaymentMethod Method, DateTime? Date);


/// <param name="Reason">A resource key, e.g. <c>invalidPaymentAmount</c> or <c>paymentExceedsAmountDue</c></param>
public sealed record PaymentRejectedResponse(string Reason);


public class CreatePaymentValidator : Validator<CreatePaymentRequest>
{
	public CreatePaymentValidator()
	{
		this.RuleFor(request => request.Amount).GreaterThan(0).WithMessage("amountNotPositive");
		this.RuleFor(request => request.Method).IsInEnum().WithMessage("paymentMethodInvalid").NotEqual(PaymentMethod.None).WithMessage("paymentMethodInvalid");
	}
}


/// <remarks>Rejects with 409 when the amount is not positive or exceeds the invoice's amount due.</remarks>
public class CreatePaymentEndpoint(IPaymentService paymentService) : Endpoint<CreatePaymentRequest, Results<Created<PaymentResponse>, NotFound, Conflict<PaymentRejectedResponse>>>
{
	private readonly IPaymentService _paymentService = paymentService;


	public override void Configure()
	{
		this.Post("/invoices/{Number}/payments");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<Results<Created<PaymentResponse>, NotFound, Conflict<PaymentRejectedResponse>>> ExecuteAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
	{
		var result = await _paymentService.RecordPaymentAsync(request.Number, request.Amount, request.Method, request.Date ?? DateTime.Now, cancellationToken);
		if (!result.Succeeded)
		{
			return result.Error switch
			{
				RecordPaymentError.InvoiceNotFound => TypedResults.NotFound(),
				RecordPaymentError.InvalidAmount => TypedResults.Conflict(new PaymentRejectedResponse("invalidPaymentAmount")),
				_ => TypedResults.Conflict(new PaymentRejectedResponse("paymentExceedsAmountDue"))
			};
		}

		return TypedResults.Created($"/api/invoices/{request.Number}/payments", PaymentResponse.From(result.Payment!));
	}
}
