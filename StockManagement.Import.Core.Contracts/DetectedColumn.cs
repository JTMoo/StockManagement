namespace StockManagement.Import.Core.Contracts;


/// <param name="Column">1-based column number, as the user sees it in the source file</param>
/// <param name="Header">The source file's header text for this column</param>
/// <param name="MatchedFieldName"><see cref="ImportField.Name"/> this header auto-matches to, or <see langword="null"/></param>
public sealed record DetectedColumn(int Column, string Header, string? MatchedFieldName);
