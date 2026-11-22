using CombinationsCore.Models;
using CombinationsCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace CombinationsStorage;

/// <summary>
/// Keeps sessions in the server's memory.
/// A session that is not used for <see cref="IdleTimeout"/> is removed automatically,
/// so abandoned sessions (e.g. after Reset or closing the browser) do not pile up.
/// </summary>
public class MemorySessionStore : ISessionStore
{
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);

    private readonly IMemoryCache _cache;

    public MemorySessionStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public CombinationSession? Get(Guid sessionId)
    {
        return _cache.TryGetValue(KeyOf(sessionId), out CombinationSession? session) ? session : null;
    }

    public void Save(CombinationSession session)
    {
        _cache.Set(KeyOf(session.Id), session, new MemoryCacheEntryOptions
        {
            SlidingExpiration = IdleTimeout,
        });
    }

    private static string KeyOf(Guid sessionId) => $"combinations-session:{sessionId}";
}
