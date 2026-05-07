namespace Jumbled.Models;

/// <summary>
/// Request model for Wordle assist queries.
/// </summary>
/// <param name="Word">The current word pattern (use '_' for unknown letters)</param>
/// <param name="Exclude">Letters to exclude from suggestions</param>
/// <param name="Include">Comma-separated letter patterns that must be included in suggestions (use '_' for unknown letters)</param>
public record WordleAssistRequest(string Word, string Exclude = "", string Include = "");
