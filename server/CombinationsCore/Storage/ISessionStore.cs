using CombinationsCore.Models;

namespace CombinationsCore.Storage;

/// <summary>
/// Where user sessions are kept.
/// The service only knows this interface, so the storage can be swapped
/// (memory today, Redis tomorrow) without touching the business logic.
/// </summary>
public interface ISessionStore
{
    /// <summary>Returns the session, or null if it does not exist (or expired).</summary>
    CombinationSession? Get(Guid sessionId);

    /// <summary>Creates or updates a session. Must be called after every change.</summary>
    void Save(CombinationSession session);
}
