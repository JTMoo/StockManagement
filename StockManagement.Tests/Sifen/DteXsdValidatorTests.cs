using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using StockManagement.Sifen.Core;

namespace StockManagement.Tests.Sifen;


[TestClass]
public sealed class DteXsdValidatorTests
{
	// This is a hand-written schema that only mirrors DteXmlBuilder's own output shape — it is NOT DNIT's
	// published SIFEN XSD, which was not available in this session (no network access to dnit.gov.py). It
	// proves the validator's plumbing; it does not prove DNIT schema conformance.
	private const string Ns = "http://ekuatia.set.gov.py/sifen/xsd";
	private const string LocalTestSchema = $"""
		<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" targetNamespace="{Ns}" xmlns="{Ns}" elementFormDefault="qualified">
		  <xs:element name="rDE">
		    <xs:complexType>
		      <xs:sequence>
		        <xs:element name="dVerFor" type="xs:string" />
		        <xs:element name="DE">
		          <xs:complexType>
		            <xs:sequence>
		              <xs:any minOccurs="0" maxOccurs="unbounded" processContents="skip" />
		            </xs:sequence>
		            <xs:attribute name="Id" type="xs:string" />
		          </xs:complexType>
		        </xs:element>
		      </xs:sequence>
		    </xs:complexType>
		  </xs:element>
		</xs:schema>
		""";

	private static XmlSchemaSet LoadTestSchema()
	{
		var schemas = new XmlSchemaSet();
		using var reader = XmlReader.Create(new StringReader(LocalTestSchema));
		schemas.Add(Ns, reader);
		return schemas;
	}

	[TestMethod]
	public void Validate_DocumentMatchingSchema_ReturnsTrueWithNoErrors()
	{
		// Arrange
		var document = XDocument.Parse($"""<rDE xmlns="{Ns}"><dVerFor>150</dVerFor><DE Id="x" /></rDE>""");

		// Act
		var isValid = new DteXsdValidator().Validate(document, LoadTestSchema(), out var errors);

		// Assert
		Assert.IsTrue(isValid);
		Assert.AreEqual(0, errors.Count);
	}

	[TestMethod]
	public void Validate_DocumentMissingRequiredElement_ReturnsFalseWithErrors()
	{
		// Arrange
		var document = XDocument.Parse($"""<rDE xmlns="{Ns}"><DE Id="x" /></rDE>""");

		// Act
		var isValid = new DteXsdValidator().Validate(document, LoadTestSchema(), out var errors);

		// Assert
		Assert.IsFalse(isValid);
		Assert.IsTrue(errors.Count > 0);
	}
}
