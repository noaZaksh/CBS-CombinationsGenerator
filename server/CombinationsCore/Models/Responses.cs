namespace CombinationsCore.Models;

// Big numbers (up to 20! = 2,432,902,008,176,640,000) are sent as strings,
// because JavaScript numbers lose precision above 9,007,199,254,740,991.

/// <summary>Result of Start.</summary>
/// <param name="SessionId">Identifies the user in all the next calls.</param>
/// <param name="N">The n that was entered.</param>
/// <param name="TotalCount">n! as a plain number string, e.g. "6".</param>
/// <param name="TotalCountText">n! formatted with commas, e.g. "2,432,902,008,176,640,000".</param>
public record StartResponse(Guid SessionId, int N, string TotalCount, string TotalCountText);

/// <summary>One combination with its serial number.</summary>
/// <param name="Number">Serial number (1-based) as a string.</param>
/// <param name="Combination">The numbers in order, e.g. [1,3,2].</param>
public record CombinationItem(string Number, int[] Combination);

/// <summary>Result of GetNext and Back.</summary>
/// <param name="HasCombination">False when there are no more combinations.</param>
/// <param name="Item">The combination to show (null when HasCombination is false).</param>
/// <param name="Message">Text to show the user when there is no combination.</param>
public record NextResponse(bool HasCombination, CombinationItem? Item, string? Message);

/// <summary>Result of GetAll: one page of combinations.</summary>
/// <param name="PageNumber">The page that was returned (1-based).</param>
/// <param name="PageSize">How many combinations per page.</param>
/// <param name="TotalPages">How many pages exist, as a string (can be huge).</param>
/// <param name="Items">Only the combinations of this page.</param>
/// <param name="Message">Text to show when there are no combinations left to list.</param>
public record PageResponse(string PageNumber, int PageSize, string TotalPages, IReadOnlyList<CombinationItem> Items, string? Message);
