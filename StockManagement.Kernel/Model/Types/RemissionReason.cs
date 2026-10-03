namespace StockManagement.Kernel.Model.Types;


/// <summary>
/// SIFEN <c>dMotTras</c> (motivo de traslado) code for a <see cref="RemissionNote"/> (#162).
/// </summary>
/// <remarks>
/// Values follow DNIT's published motivo-de-traslado code list; not cross-checked against an official DNIT
/// example in this session (no network access to dnit.gov.py) - same flag as every other unverified SIFEN
/// constant in this project.
/// </remarks>
public enum RemissionReason
{
	Venta = 1,
	Consignacion = 2,
	Reposicion = 3,
	TrasladoEntreLocales = 8,
	Otro = 9,
}
