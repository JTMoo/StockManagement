using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Api.Features.Sifen;


/// <remarks>Required surface for #149 - lets the counter see whether contingency mode is on and how much range is left.</remarks>
public class GetContingencyStatusEndpoint(IContingencyCdcIssuer contingencyCdcIssuer) : EndpointWithoutRequest<ContingencyStatusResponse>
{
	private readonly IContingencyCdcIssuer _contingencyCdcIssuer = contingencyCdcIssuer;


	public override void Configure()
	{
		this.Get("/sifen/contingency-mode");
		this.Permissions(Permission.SalesRead);
	}

	public override async Task<ContingencyStatusResponse> ExecuteAsync(CancellationToken cancellationToken)
	{
		return ContingencyStatusResponse.From(await _contingencyCdcIssuer.GetStatusAsync(cancellationToken));
	}
}
