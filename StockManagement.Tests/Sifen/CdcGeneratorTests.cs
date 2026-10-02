using StockManagement.Kernel.Util;
using StockManagement.Sifen.Core;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Tests.Sifen;


[TestClass]
public sealed class CdcGeneratorTests
{
	// No official DNIT worked example was available in this session (no network access to dnit.gov.py) to use
	// as a hardcoded test vector; these tests instead verify the documented field layout and cross-check the
	// check digit against an independent Mod11 computation (same check-digit family as RucValidator/ADR-0021).
	private static readonly CdcInput SampleInput = new(
		DocumentType: SifenDocumentType.FacturaElectronica,
		RucBase: "1946520",
		RucCheckDigit: 3,
		EstablishmentCode: "001",
		PointOfSaleCode: "002",
		DocumentNumber: 123,
		TaxpayerType: TaxpayerType.Juridica,
		IssueDate: new DateOnly(2026, 10, 2),
		EmissionType: EmissionType.Normal,
		SecurityCode: "123456789");

	[TestMethod]
	public void Generate_ValidInput_Returns44Digits()
	{
		// Act
		var cdc = new CdcGenerator().Generate(SampleInput);

		// Assert
		Assert.AreEqual(44, cdc.Length);
		Assert.IsTrue(cdc.All(char.IsDigit));
	}

	[TestMethod]
	public void Generate_ValidInput_EncodesEachFieldAtItsDocumentedPosition()
	{
		// Act
		var cdc = new CdcGenerator().Generate(SampleInput);

		// Assert
		Assert.AreEqual("01", cdc[..2]); // iTiDE
		Assert.AreEqual("01946520", cdc[2..10]); // RUC, zero-padded to 8
		Assert.AreEqual("3", cdc[10..11]); // RUC check digit
		Assert.AreEqual("001", cdc[11..14]); // establishment
		Assert.AreEqual("002", cdc[14..17]); // point of sale
		Assert.AreEqual("0000123", cdc[17..24]); // document number
		Assert.AreEqual("2", cdc[24..25]); // taxpayer type (Juridica)
		Assert.AreEqual("20261002", cdc[25..33]); // issue date
		Assert.AreEqual("1", cdc[33..34]); // emission type (Normal)
		Assert.AreEqual("123456789", cdc[34..43]); // security code
	}

	[TestMethod]
	public void Generate_ValidInput_LastDigitIsIndependentlyVerifiableMod11CheckDigit()
	{
		// Act
		var cdc = new CdcGenerator().Generate(SampleInput);

		// Assert
		var expectedCheckDigit = Mod11.ComputeCheckDigit(cdc[..43]);
		Assert.AreEqual(expectedCheckDigit, cdc[43] - '0');
	}

	[TestMethod]
	public void Generate_SameInput_IsDeterministic()
	{
		// Act
		var first = new CdcGenerator().Generate(SampleInput);
		var second = new CdcGenerator().Generate(SampleInput);

		// Assert
		Assert.AreEqual(first, second);
	}

	[TestMethod]
	public void Generate_DifferentDocumentNumber_ChangesCdc()
	{
		// Act
		var first = new CdcGenerator().Generate(SampleInput);
		var second = new CdcGenerator().Generate(SampleInput with { DocumentNumber = 124 });

		// Assert
		Assert.AreNotEqual(first, second);
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("123456789")]
	public void Generate_InvalidRucBase_Throws(string rucBase)
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new CdcGenerator().Generate(SampleInput with { RucBase = rucBase }));
	}

	[TestMethod]
	[DataRow("1")]
	[DataRow("abc")]
	public void Generate_InvalidEstablishmentCode_Throws(string establishmentCode)
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new CdcGenerator().Generate(SampleInput with { EstablishmentCode = establishmentCode }));
	}

	[TestMethod]
	[DataRow(0L)]
	[DataRow(10000000L)]
	public void Generate_DocumentNumberOutOfRange_Throws(long documentNumber)
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new CdcGenerator().Generate(SampleInput with { DocumentNumber = documentNumber }));
	}

	[TestMethod]
	public void Generate_SecurityCodeNot9Digits_Throws()
	{
		// Act + Assert
		Assert.ThrowsException<ArgumentException>(() => new CdcGenerator().Generate(SampleInput with { SecurityCode = "123" }));
	}
}
