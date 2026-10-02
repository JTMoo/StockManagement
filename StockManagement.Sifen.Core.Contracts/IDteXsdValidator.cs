using System.Xml.Linq;
using System.Xml.Schema;

namespace StockManagement.Sifen.Core.Contracts;


public interface IDteXsdValidator
{
	/// <summary>
	/// Validates <paramref name="document"/> against <paramref name="schemas"/>, collecting every violation into <paramref name="errors"/>.
	/// </summary>
	bool Validate(XDocument document, XmlSchemaSet schemas, out IReadOnlyList<string> errors);
}
