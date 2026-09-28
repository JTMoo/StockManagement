namespace StockManagement.Import.Core.Contracts;


/// <param name="Name">The target's C# property name; stable identity for a column mapping</param>
/// <param name="DisplayName">Localized <c>[Display]</c> value, for the web mapping UI</param>
public sealed record ImportField(string Name, string DisplayName);
