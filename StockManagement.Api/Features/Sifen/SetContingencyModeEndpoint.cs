using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Api.Features.Sifen;


/// <summary>
/// Operator declares the start or end of a contingency event (#149). Active while issuing offline from the
/// pre-assigned range; turned off once connectivity is restored - the outbox still transmits within 72h regardless.
/// </summary>
public sealed record SetContingencyModeRequest(bool Active);


public class SetContingencyModeEndpoint(IContingencyCdcIssuer contingencyCdcIssuer) : Endpoint<SetContingencyModeRequest, ContingencyStatusResponse>
{
	private readonly IContingencyCdcIssuer _contingencyCdcIssuer = contingencyCdcIssuer;


	public override void Configure()
	{
		this.Post("/sifen/contingency-mode");
		this.Permissions(Permission.SalesWrite);
	}

	public override async Task<ContingencyStatusResponse> ExecuteAsync(SetContingencyModeRequest request, CancellationToken cancellationToken)
	{
		await _contingencyCdcIssuer.SetActiveAsync(request.Active, cancellationToken);
		return ContingencyStatusResponse.From(await _contingencyCdcIssuer.GetStatusAsync(cancellationToken));
	}
}
