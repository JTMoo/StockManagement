using FastEndpoints;
using FluentValidation;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Api.Features.Sifen;


/// <summary>
/// Stores the DNIT-granted contingency CDC range (#149); obtained from DNIT ahead of an outage, entered here once.
/// Replaces any previous range and turns contingency mode off - use <see cref="SetContingencyModeEndpoint"/> to start it.
/// </summary>
public sealed record SetContingencyRangeRequest(long RangeStart, long RangeEnd);


public class SetContingencyRangeValidator : Validator<SetContingencyRangeRequest>
{
	public SetContingencyRangeValidator()
	{
		this.RuleFor(request => request.RangeStart).GreaterThan(0).WithMessage("rangeStartNotPositive");
		this.RuleFor(request => request.RangeEnd).GreaterThan(request => request.RangeStart).WithMessage("rangeEndNotAfterStart");
	}
}


public class SetContingencyRangeEndpoint(IContingencyCdcIssuer contingencyCdcIssuer) : Endpoint<SetContingencyRangeRequest, ContingencyStatusResponse>
{
	private readonly IContingencyCdcIssuer _contingencyCdcIssuer = contingencyCdcIssuer;


	public override void Configure()
	{
		this.Put("/sifen/contingency-range");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<ContingencyStatusResponse> ExecuteAsync(SetContingencyRangeRequest request, CancellationToken cancellationToken)
	{
		await _contingencyCdcIssuer.ConfigureRangeAsync(request.RangeStart, request.RangeEnd, cancellationToken);
		return ContingencyStatusResponse.From(await _contingencyCdcIssuer.GetStatusAsync(cancellationToken));
	}
}
