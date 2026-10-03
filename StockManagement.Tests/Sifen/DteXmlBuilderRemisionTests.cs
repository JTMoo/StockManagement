using System.Xml.Linq;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sifen.Core;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Tests.Sifen;


[TestClass]
public sealed class DteXmlBuilderRemisionTests
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

	private static DteRemisionData BuildData(DteReceptor? receptor = null, IReadOnlyList<DteRemisionItem>? items = null, RemissionReason reason = RemissionReason.TrasladoEntreLocales) => new(
		Cdc: ValidCdc,
		Emisor: Emisor,
		Receptor: receptor ?? new DteReceptor("Juan Perez", "80012345", 6, null),
		IssueDate: new DateTime(2026, 10, 3, 9, 30, 0),
		DocumentNumber: 123,
		Reason: reason,
		DestinationAddress: "Avda. España 500",
		Items: items ?? [new DteRemisionItem("SKU-1", "Widget", 2)]);

	[TestMethod]
	public void BuildRemision_ValidData_SetsDeIdToDePrefixedCdc()
	{
		// Act
		var doc = new DteXmlBuilder().BuildRemision(BuildData());

		// Assert
		var de = doc.Root!.Element(Ns + "DE")!;
		Assert.AreEqual("DE" + ValidCdc, de.Attribute("Id")!.Value);
	}

	[TestMethod]
	public void BuildRemision_ValidData_UsesNotaDeRemisionDocumentType()
	{
		// Act
		var doc = new DteXmlBuilder().BuildRemision(BuildData());

		// Assert
		var gTimb = doc.Descendants(Ns + "gTimb").Single();
		Assert.AreEqual(((int)SifenDocumentType.NotaDeRemisionElectronica).ToString(), gTimb.Element(Ns + "iTiDE")!.Value);
	}

	[TestMethod]
	public void BuildRemision_ValidData_EncodesOneGroupPerItemWithNoPriceOrVat()
	{
		// Act
		var doc = new DteXmlBuilder().BuildRemision(BuildData(items: [
			new DteRemisionItem("A", "Item A", 1),
			new DteRemisionItem("B", "Item B", 2),
		]));

		// Assert
		var items = doc.Descendants(Ns + "gCamItem").ToList();
		Assert.AreEqual(2, items.Count);
		Assert.IsNull(items[0].Element(Ns + "gCamIVA"));
		Assert.IsNull(items[0].Element(Ns + "gValorItem"));
	}

	[TestMethod]
	public void BuildRemision_ValidData_EncodesMotivoAndDestination()
	{
		// Act
		var doc = new DteXmlBuilder().BuildRemision(BuildData(reason: RemissionReason.Venta));

		// Assert
		var gTransp = doc.Descendants(Ns + "gTransp").Single();
		Assert.AreEqual(((int)RemissionReason.Venta).ToString(), gTransp.Element(Ns + "iMotTras")!.Value);
		Assert.AreEqual("Avda. España 500", gTransp.Element(Ns + "dDirDest")!.Value);
	}

	[TestMethod]
	public void BuildRemision_ReceptorWithoutRuc_UsesDocumentNumber()
	{
		// Act
		var doc = new DteXmlBuilder().BuildRemision(BuildData(receptor: new DteReceptor("Juan Perez", null, null, "3456789")));

		// Assert
		var datRec = doc.Descendants(Ns + "gDatRec").Single();
		Assert.AreEqual("3456789", datRec.Element(Ns + "dNumIDRec")!.Value);
		Assert.IsNull(datRec.Element(Ns + "dRucRec"));
	}

	[TestMethod]
	public void BuildRemision_NoItems_Throws()
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new DteXmlBuilder().BuildRemision(BuildData(items: [])));
	}

	[TestMethod]
	public void BuildRemision_CdcNot44Digits_Throws()
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new DteXmlBuilder().BuildRemision(BuildData() with { Cdc = "123" }));
	}
}
