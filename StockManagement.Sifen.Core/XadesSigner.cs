using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Sifen.Core;


/// <summary>
/// Adds an enveloped XAdES-BES <c>ds:Signature</c> (RSA-SHA256, Exclusive C14N) to a DE's root element, with a
/// <c>xades:SignedProperties</c> (signing time + signing-certificate digest) covered by a second signed reference.
/// </summary>
/// <remarks>
/// Covers the XAdES-BES form (signed properties, no timestamp/archival extensions) — DNIT's exact required
/// XAdES level/profile has not been confirmed against the published manual in this session (no network access
/// to dnit.gov.py); round-trip signature validity is covered by <c>XadesSignerTests</c>, but the DNIT-specific
/// shape is not.
/// </remarks>
public sealed class XadesSigner : IXadesSigner
{
	private const string XadesNs = "http://uri.etsi.org/01903/v1.3.2#";
	private const string DsNs = "http://www.w3.org/2000/09/xmldsig#";

	public XDocument Sign(XDocument unsignedDe, X509Certificate2 certificate)
	{
		ArgumentNullException.ThrowIfNull(unsignedDe);
		ArgumentNullException.ThrowIfNull(certificate);

		var xmlDoc = new XmlDocument { PreserveWhitespace = true };
		xmlDoc.LoadXml(unsignedDe.ToString(SaveOptions.DisableFormatting));

		var deElement = xmlDoc.GetElementsByTagName("DE").Cast<XmlElement>().FirstOrDefault()
			?? throw new InvalidOperationException("DE element not found.");
		var deId = deElement.GetAttribute("Id");
		var signatureId = $"Signature-{deId}";
		var signedPropertiesId = $"SignedProperties-{deId}";

		var signedXml = new XadesSignedXml(xmlDoc) { SigningKey = certificate.GetRSAPrivateKey() };
		signedXml.Signature.Id = signatureId;
		// Exclusive (not plain) C14N: the SignedProperties Object is built detached, with no ancestor namespace
		// context, then embedded deep in the DE tree afterwards — plain/inclusive C14N pulls in the ancestors'
		// in-scope namespaces at that point, so its canonical output (and therefore the digest) depends on where
		// in the tree the element ends up, which breaks verification after a round-trip through XML text.
		signedXml.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

		var deReference = new Reference($"#{deId}");
		deReference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
		deReference.AddTransform(new XmlDsigExcC14NTransform());
		signedXml.AddReference(deReference);

		var qualifyingProperties = BuildQualifyingProperties(xmlDoc, certificate, signatureId, signedPropertiesId);
		var fragment = xmlDoc.CreateDocumentFragment();
		fragment.AppendChild(qualifyingProperties);
		signedXml.AddObject(new DataObject { Data = fragment.ChildNodes });

		var signedPropertiesReference = new Reference($"#{signedPropertiesId}")
		{
			Type = "http://uri.etsi.org/01903#SignedProperties",
		};
		signedPropertiesReference.AddTransform(new XmlDsigExcC14NTransform());
		signedXml.AddReference(signedPropertiesReference);

		signedXml.KeyInfo = new KeyInfo();
		signedXml.KeyInfo.AddClause(new KeyInfoX509Data(certificate));

		signedXml.ComputeSignature();
		deElement.AppendChild(xmlDoc.ImportNode(signedXml.GetXml(), true));

		return XDocument.Parse(xmlDoc.OuterXml);
	}

	private static XmlElement BuildQualifyingProperties(XmlDocument xmlDoc, X509Certificate2 certificate, string signatureId, string signedPropertiesId)
	{
		var qualifyingProperties = xmlDoc.CreateElement("xades", "QualifyingProperties", XadesNs);
		qualifyingProperties.SetAttribute("Target", $"#{signatureId}");

		var signedProperties = xmlDoc.CreateElement("xades", "SignedProperties", XadesNs);
		signedProperties.SetAttribute("Id", signedPropertiesId);

		var signedSignatureProperties = xmlDoc.CreateElement("xades", "SignedSignatureProperties", XadesNs);

		var signingTime = xmlDoc.CreateElement("xades", "SigningTime", XadesNs);
		signingTime.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
		signedSignatureProperties.AppendChild(signingTime);

		var cert = xmlDoc.CreateElement("xades", "Cert", XadesNs);

		var certDigest = xmlDoc.CreateElement("xades", "CertDigest", XadesNs);
		var digestMethod = xmlDoc.CreateElement("ds", "DigestMethod", DsNs);
		digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");
		var digestValue = xmlDoc.CreateElement("ds", "DigestValue", DsNs);
		digestValue.InnerText = Convert.ToBase64String(SHA256.HashData(certificate.RawData));
		certDigest.AppendChild(digestMethod);
		certDigest.AppendChild(digestValue);

		var issuerSerial = xmlDoc.CreateElement("xades", "IssuerSerial", XadesNs);
		var issuerName = xmlDoc.CreateElement("ds", "X509IssuerName", DsNs);
		issuerName.InnerText = certificate.IssuerName.Name;
		var serialNumber = xmlDoc.CreateElement("ds", "X509SerialNumber", DsNs);
		serialNumber.InnerText = new System.Numerics.BigInteger(certificate.GetSerialNumber()).ToString();
		issuerSerial.AppendChild(issuerName);
		issuerSerial.AppendChild(serialNumber);

		cert.AppendChild(certDigest);
		cert.AppendChild(issuerSerial);

		var signingCertificate = xmlDoc.CreateElement("xades", "SigningCertificate", XadesNs);
		signingCertificate.AppendChild(cert);
		signedSignatureProperties.AppendChild(signingCertificate);

		signedProperties.AppendChild(signedSignatureProperties);
		qualifyingProperties.AppendChild(signedProperties);
		return qualifyingProperties;
	}

	/// <summary>
	/// <see cref="SignedXml"/> only resolves "#id" references against elements already in the document; a
	/// <c>SignedProperties</c> added via <see cref="SignedXml.AddObject"/> lives in the not-yet-attached
	/// <c>ds:Object</c>, so reference resolution is extended to search there too.
	/// </summary>
	private sealed class XadesSignedXml(XmlDocument document) : SignedXml(document)
	{
		public override XmlElement? GetIdElement(XmlDocument? document, string idValue)
		{
			return base.GetIdElement(document, idValue) ?? FindInObjects(idValue);
		}

		private XmlElement? FindInObjects(string idValue)
		{
			foreach (DataObject dataObject in Signature.ObjectList)
			{
				foreach (XmlNode node in dataObject.Data)
				{
					if (node is XmlElement element && FindById(element, idValue) is { } match)
						return match;
				}
			}
			return null;
		}

		private static XmlElement? FindById(XmlElement element, string idValue)
		{
			if (element.GetAttribute("Id") == idValue) return element;
			foreach (XmlNode child in element.ChildNodes)
			{
				if (child is XmlElement childElement && FindById(childElement, idValue) is { } match)
					return match;
			}
			return null;
		}
	}
}
