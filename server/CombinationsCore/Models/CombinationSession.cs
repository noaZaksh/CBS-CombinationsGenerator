namespace CombinationsCore.Models;

/// <summary>
/// Everything the server remembers about one user.
/// Only a few numbers are kept — never a list of combinations.
/// </summary>
public class CombinationSession
{
    public Guid Id { get; init; }

    /// <summary>The n the user entered in Start.</summary>
    public int N { get; init; }

    /// <summary>n! — how many combinations exist.</summary>
    public long TotalCount { get; init; }

    /// <summary>
    /// Serial number (1-based) of the combination currently shown on the main screen.
    /// 0 means "Next" was not pressed yet.
    /// The "all combinations" list always starts right after this number.
    /// </summary>
    public long CurrentNumber { get; set; }

    /// <summary>The combination currently shown (null if nothing was shown yet).</summary>
    public int[]? CurrentCombination { get; set; }

    /// <summary>
    /// Serial number of the last combination shown on the last page of the
    /// "all combinations" list. 0 means no page was shown yet.
    /// Used by "Back" to return to that combination.
    /// </summary>
    public long LastShownNumber { get; set; }
}
