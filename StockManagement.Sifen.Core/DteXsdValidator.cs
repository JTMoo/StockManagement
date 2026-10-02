using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Sifen.Core;


/// <summary>
/// Validates a DTE document against a supplied <see cref="XmlSchemaSet"/>.
/// </summary>
/// <remarks>
/// Takes the schema set as a parameter rather than embedding one: DNIT's real XSD files were not fetched in
/// this session (no network access to dnit.gov.py). Load the actual published XSDs (e.g. via
/// <see cref="XmlSchemaSet.Add(string?, string)"/>) before using this against real DTEs; until then,
/// <c>DteXsdValidatorTests</c> only checks this method's structural validation logic against a local test schema.
/// </remarks>
public sealed class DteXsdValidator : IDteXsdValidator
{
	public bool Validate(XDocument document, XmlSchemaSet schemas, out IReadOnlyList<string> errors)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(schemas);

		var messages = new List<string>();
		var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = schemas };
		settings.ValidationEventHandler += (_, e) => messages.Add(e.Message);

		using var nodeReader = document.CreateReader();
		using var validatingReader = XmlReader.Create(nodeReader, settings);
		while (validatingReader.Read()) { }

		errors = messages;
		return messages.Count == 0;
	}
}
