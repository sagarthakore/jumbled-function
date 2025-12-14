namespace Jumbled.Models;

/// <summary>
/// Request model for Wordle assist queries.
/// </summary>
/// <param name="Word">The current word pattern (use '.' for unknown letters)</param>
/// <param name="Exclude">Letters to exclude from suggestions</param>
/// <param name="Include">Letters that must be included in suggestions</param>
public record WordleAssistRequest(string Word, string Exclude, string Include);
