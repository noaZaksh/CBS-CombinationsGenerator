using System.Globalization;
using CombinationsCore.Algorithms;
using CombinationsCore.Models;
using CombinationsCore.Storage;

namespace CombinationsCore.Services;

/// <summary>
/// The business logic of the combinations generator.
/// It never stores a list of combinations: only the current one and a few numbers.
/// </summary>
public class CombinationsService : ICombinationsService
{
    public const string NoMoreCombinationsMessage = "אין יותר קומבינציות";
    public const int MaxPageSize = 1000;

    private readonly ISessionStore _store;

    public CombinationsService(ISessionStore store)
    {
        _store = store;
    }

    public StartResponse Start(int n)
    {
        if (n < PermutationCalculator.MinN || n > PermutationCalculator.MaxN)
        {
            throw new ArgumentException($"n must be a whole number between {PermutationCalculator.MinN} and {PermutationCalculator.MaxN}.");
        }

        var session = new CombinationSession
        {
            Id = Guid.NewGuid(),
            N = n,
            TotalCount = PermutationCalculator.CountPermutations(n),
        };
        _store.Save(session);

        return new StartResponse(session.Id, n, ToText(session.TotalCount), WithCommas(session.TotalCount));
    }

    public NextResponse GetNext(Guid sessionId)
    {
        var session = LoadSession(sessionId);
        lock (session) // protects against two quick clicks from the same user
        {
            if (session.CurrentCombination is null)
            {
                // First click: the first combination [1, 2, ..., n]
                session.CurrentCombination = PermutationCalculator.GetFirst(session.N);
            }
            else
            {
                // Work on a copy, so the session changes only if there really is a next one.
                var next = (int[])session.CurrentCombination.Clone();
                if (!PermutationCalculator.TryMoveToNext(next))
                {
                    return new NextResponse(false, null, NoMoreCombinationsMessage);
                }
                session.CurrentCombination = next;
            }

            session.CurrentNumber++;
            _store.Save(session);

            return new NextResponse(true, ToItem(session.CurrentNumber, session.CurrentCombination), null);
        }
    }

    public PageResponse GetAll(Guid sessionId, int pageSize, long pageNumber)
    {
        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new ArgumentException($"pageSize must be between 1 and {MaxPageSize}.");
        }

        var session = LoadSession(sessionId);
        lock (session)
        {
            // The list starts right after the combination shown on the main screen.
            long remaining = session.TotalCount - session.CurrentNumber;
            if (remaining == 0)
            {
                return new PageResponse("0", pageSize, "0", Array.Empty<CombinationItem>(), NoMoreCombinationsMessage);
            }

            long totalPages = (remaining + pageSize - 1) / pageSize;
            if (pageNumber < 1 || pageNumber > totalPages)
            {
                throw new ArgumentException($"pageNumber must be between 1 and {totalPages}.");
            }

            long firstNumber = session.CurrentNumber + (pageNumber - 1) * pageSize + 1;
            long lastNumber = Math.Min(firstNumber + pageSize - 1, session.TotalCount);

            // Jump straight to the first combination of the page (index-based),
            // then walk to the end of the page with NextPermutation.
            var combination = PermutationCalculator.GetByIndex(session.N, firstNumber - 1);
            var items = new List<CombinationItem>((int)(lastNumber - firstNumber + 1));
            for (long number = firstNumber; number <= lastNumber; number++)
            {
                items.Add(ToItem(number, combination));
                if (number < lastNumber)
                {
                    PermutationCalculator.TryMoveToNext(combination);
                }
            }

            // Remember the last combination on screen, for the "Back" button.
            session.LastShownNumber = lastNumber;
            _store.Save(session);

            return new PageResponse(ToText(pageNumber), pageSize, ToText(totalPages), items, null);
        }
    }

    public NextResponse Back(Guid sessionId)
    {
        var session = LoadSession(sessionId);
        lock (session)
        {
            // If a page was shown, its last combination becomes the current one.
            if (session.LastShownNumber > 0)
            {
                session.CurrentNumber = session.LastShownNumber;
                session.CurrentCombination = PermutationCalculator.GetByIndex(session.N, session.LastShownNumber - 1);
                session.LastShownNumber = 0;
                _store.Save(session);
            }

            if (session.CurrentCombination is null)
            {
                // Nothing was ever shown (no "Next" and no page) – nothing to display.
                return new NextResponse(false, null, null);
            }

            return new NextResponse(true, ToItem(session.CurrentNumber, session.CurrentCombination), null);
        }
    }

    private CombinationSession LoadSession(Guid sessionId)
    {
        return _store.Get(sessionId) ?? throw new SessionNotFoundException(sessionId);
    }

    private static CombinationItem ToItem(long number, int[] combination)
    {
        return new CombinationItem(ToText(number), (int[])combination.Clone());
    }

    private static string ToText(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static string WithCommas(long value) => value.ToString("N0", CultureInfo.InvariantCulture);
}
