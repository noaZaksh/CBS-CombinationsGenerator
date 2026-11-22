namespace CombinationsCore.Services;

/// <summary>Thrown when a session id is unknown or the session expired.</summary>
public class SessionNotFoundException : Exception
{
    public SessionNotFoundException(Guid sessionId)
        : base($"Session {sessionId} was not found or has expired. Please start again.")
    {
    }
}
