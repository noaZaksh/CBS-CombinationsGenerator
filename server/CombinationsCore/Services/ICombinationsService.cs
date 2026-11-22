using CombinationsCore.Models;

namespace CombinationsCore.Services;

/// <summary>The four actions the user can do. The controller only calls these.</summary>
public interface ICombinationsService
{
    /// <summary>Starts a new session for n and returns how many combinations exist.</summary>
    StartResponse Start(int n);

    /// <summary>Returns the next combination after the current one (computes only that one).</summary>
    NextResponse GetNext(Guid sessionId);

    /// <summary>Returns one page of the combinations that come after the current one.</summary>
    PageResponse GetAll(Guid sessionId, int pageSize, long pageNumber);

    /// <summary>Leaves the list: the last combination shown in the list becomes the current one.</summary>
    NextResponse Back(Guid sessionId);
}
