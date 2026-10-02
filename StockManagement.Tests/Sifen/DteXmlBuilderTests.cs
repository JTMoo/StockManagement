using System.Xml.Linq;
using StockManagement.Sifen.Core;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Tests.Sifen;


[TestClass]
public sealed class DteXmlBuilderTests
{
	private static readonly XNamespace Ns = "http://ekuatia.set.gov.py/sifen/xsd";
	private static readonly string ValidCdc = new('1', 44);

	private static readonly DteEmisor Emisor = new(
		RucBase: "1946520",
		RucCheckDigit: 3,
		RazonSocial: "Acme S.A.",
		EstablishmentCode: "001",
		PointOfSaleCode: "001",
		EstablishmentAddress: "Avda. Mcal. López 1234",
		TimbradoNumber: "12345678",
		TimbradoValidSince: new DateOnly(2026, 1, 1));

	private static DteInvoiceData BuildData(DteReceptor? receptor = null, IReadOnlyList<DteItem>? items = null) => new(
		Cdc: ValidCdc,
		Emisor: Emisor,
		Receptor: receptor ?? new DteReceptor("Juan Perez", "80012345", 6, null),
		IssueDate: new DateTime(2026, 10, 2, 9, 30, 0),
		DocumentNumber: 123,
		Items: items ?? [new DteItem("SKU-1", "Widget", 2, 55000m, 10m)],
		CurrencyDecimalDigits: 0);

	[TestMethod]
	public void BuildInvoice_ValidData_SetsDeIdToDePrefixedCdc()
	{
		// Act
		var doc = new DteXmlBuilder().BuildInvoice(BuildData());

		// Assert
		var de = doc.Root!.Element(Ns + "DE")!;
		Assert.AreEqual("DE" + ValidCdc, de.Attribute("Id")!.Value);
	}

	[TestMethod]
	public void BuildInvoice_ValidData_EncodesOneGroupPerItem()
	{
		// Act
		var doc = new DteXmlBuilder().BuildInvoice(BuildData(items: [
			new DteItem("A", "Item A", 1, 10000m, 10m),
			new DteItem("B", "Item B", 2, 5000m, 5m),
		]));

		// Assert
		var items = doc.Descendants(Ns + "gCamItem").ToList();
		Assert.AreEqual(2, items.Count);
	}

	[TestMethod]
	public void BuildInvoice_ReceptorWithRuc_UsesRucFields()
	{
		// Act
		var doc = new DteXmlBuilder().BuildInvoice(BuildData(receptor: new DteReceptor("Juan Perez", "80012345", 6, null)));

		// Assert
		var datRec = doc.Descendants(Ns + "gDatRec").Single();
		Assert.AreEqual("80012345", datRec.Element(Ns + "dRucRec")!.Value);
		Assert.IsNull(datRec.Element(Ns + "dNumIDRec"));
	}

	[TestMethod]
	public void BuildInvoice_ReceptorWithoutRuc_UsesDocumentNumber()
	{
		// Act
		var doc = new DteXmlBuilder().BuildInvoice(BuildData(receptor: new DteReceptor("Juan Perez", null, null, "3456789")));

		// Assert
		var datRec = doc.Descendants(Ns + "gDatRec").Single();
		Assert.AreEqual("3456789", datRec.Element(Ns + "dNumIDRec")!.Value);
		Assert.IsNull(datRec.Element(Ns + "dRucRec"));
	}

	[TestMethod]
	public void BuildInvoice_MixedVatRates_SplitsTotalsPerRate()
	{
		// Act
		var doc = new DteXmlBuilder().BuildInvoice(BuildData(items: [
			new DteItem("A", "10% item", 1, 11000m, 10m), // VAT = 1000
			new DteItem("B", "5% item", 1, 10500m, 5m),   // VAT = 500
			new DteItem("C", "exempt item", 1, 2000m, 0m),
		]));

		// Assert
		var totSub = doc.Descendants(Ns + "gTotSub").Single();
		Assert.AreEqual("11000", totSub.Element(Ns + "dSub10")!.Value);
		Assert.AreEqual("10500", totSub.Element(Ns + "dSub5")!.Value);
		Assert.AreEqual("2000", totSub.Element(Ns + "dSubExe")!.Value);
		Assert.AreEqual("1000", totSub.Element(Ns + "dIVA10")!.Value);
		Assert.AreEqual("500", totSub.Element(Ns + "dIVA5")!.Value);
		Assert.AreEqual("1500", totSub.Element(Ns + "dTotIVA")!.Value);
		Assert.AreEqual("23500", totSub.Element(Ns + "dTotOpe")!.Value);
	}

	[TestMethod]
	public void BuildInvoice_NoItems_Throws()
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new DteXmlBuilder().BuildInvoice(BuildData(items: [])));
	}

	[TestMethod]
	public void BuildInvoice_CdcNot44Digits_Throws()
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new DteXmlBuilder().BuildInvoice(BuildData() with { Cdc = "123" }));
	}
}
