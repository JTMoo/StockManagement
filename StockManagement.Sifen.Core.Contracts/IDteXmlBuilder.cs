using System.Xml.Linq;

namespace StockManagement.Sifen.Core.Contracts;


public interface IDteXmlBuilder
{
	/// <summary>
	/// Builds the unsigned <c>rDE</c> XML document for a Factura Electrónica.
	/// </summary>
	XDocument BuildInvoice(DteInvoiceData data);
}
