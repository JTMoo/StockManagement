using System.Reflection;
using System.Xml.Linq;

namespace StockManagement.Tests.Language;

[TestClass]
public class ResxDesignerSyncTests
{
	[TestMethod]
	public void Resx_And_Designer_Keys_Match_ForEveryDomainAndCulture()
	{
		var languageDir = FindLanguageDirectory();
		var mismatches = new List<string>();

		foreach (var designerType in typeof(StockManagement.Language.Invoices).Assembly.GetTypes()
			.Where(t => t.Namespace == "StockManagement.Language" && t.IsClass))
		{
			var domain = designerType.Name;
			var domainDir = Path.Combine(languageDir, domain);
			if (!Directory.Exists(domainDir))
				continue;

			var designerKeys = designerType
				.GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(p => p.PropertyType == typeof(string))
				.Select(p => p.Name)
				.ToHashSet();

			foreach (var resxPath in Directory.GetFiles(domainDir, $"{domain}*.resx"))
			{
				var resxKeys = XDocument.Load(resxPath).Root!
					.Elements("data")
					.Select(e => e.Attribute("name")!.Value)
					.ToHashSet();

				var missingInResx = designerKeys.Except(resxKeys).ToList();
				var missingInDesigner = resxKeys.Except(designerKeys).ToList();

				if (missingInResx.Count > 0)
					mismatches.Add($"{Path.GetFileName(resxPath)}: Designer.cs has {string.Join(", ", missingInResx)} but resx does not");
				if (missingInDesigner.Count > 0)
					mismatches.Add($"{Path.GetFileName(resxPath)}: resx has {string.Join(", ", missingInDesigner)} but Designer.cs does not");
			}
		}

		Assert.IsTrue(mismatches.Count == 0, string.Join("\n", mismatches));
	}

	private static string FindLanguageDirectory()
	{
		var dir = AppContext.BaseDirectory;
		while (dir is not null)
		{
			var candidate = Path.Combine(dir, "StockManagement.Language");
			if (Directory.Exists(candidate))
				return candidate;
			dir = Path.GetDirectoryName(dir);
		}

		throw new DirectoryNotFoundException("Could not locate StockManagement.Language directory from test base directory.");
	}
}
