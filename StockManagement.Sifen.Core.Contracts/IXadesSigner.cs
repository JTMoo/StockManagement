using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace StockManagement.Sifen.Core.Contracts;


public interface IXadesSigner
{
	/// <summary>
	/// Returns a copy of <paramref name="unsignedDe"/> with an enveloped XAdES-BES <c>ds:Signature</c> added to its <c>DE</c> element.
	/// </summary>
	XDocument Sign(XDocument unsignedDe, X509Certificate2 certificate);
}
