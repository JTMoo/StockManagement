namespace StockManagement.Import.Core.Contracts;


/// <summary>
/// A target field a source column can be mapped to
/// </summary>
/// <param name="Name">The target's C# property name; stable identity for a column mapping</param>
/// <param name="DisplayName">Localized <c>[Display]</c> value, for the web mapping UI</param>
public sealed record ImportField(string Name, string DisplayName);
