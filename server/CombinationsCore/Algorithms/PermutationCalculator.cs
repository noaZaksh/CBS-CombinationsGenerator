namespace CombinationsCore.Algorithms;

/// <summary>
/// Pure math for permutations of the numbers 1..n in lexicographic order.
/// Nothing here stores state, so it is safe to use from any thread.
/// </summary>
public static class PermutationCalculator
{
    public const int MinN = 1;
    public const int MaxN = 20; // 20! still fits in a long (21! does not)

    /// <summary>Returns n! — the number of ways to order n numbers in a row.</summary>
    public static long CountPermutations(int n)
    {
        ValidateN(n);

        long result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    /// <summary>The first permutation: [1, 2, ..., n].</summary>
    public static int[] GetFirst(int n)
    {
        ValidateN(n);
        return Enumerable.Range(1, n).ToArray();
    }

    /// <summary>
    /// "Next Permutation": turns <paramref name="current"/> into the next permutation
    /// in lexicographic order, in place. O(n) time, O(1) extra memory.
    /// Returns false if <paramref name="current"/> is already the last permutation.
    /// </summary>
    public static bool TryMoveToNext(int[] current)
    {
        // 1. From the right, find the first position where the value goes "up".
        int pivot = current.Length - 2;
        while (pivot >= 0 && current[pivot] >= current[pivot + 1])
        {
            pivot--;
        }

        if (pivot < 0)
        {
            return false; // fully descending => this was the last permutation
        }

        // 2. From the right, find the first value bigger than the pivot and swap them.
        int swapWith = current.Length - 1;
        while (current[swapWith] <= current[pivot])
        {
            swapWith--;
        }
        (current[pivot], current[swapWith]) = (current[swapWith], current[pivot]);

        // 3. Reverse the tail so it becomes the smallest possible ordering.
        Array.Reverse(current, pivot + 1, current.Length - pivot - 1);
        return true;
    }

    /// <summary>
    /// "Index-based permutation": builds the permutation at a given position directly,
    /// without walking through all the permutations before it.
    /// Uses the factorial number system. O(n^2) time — for n ≤ 20 that is instant.
    /// </summary>
    /// <param name="n">How many numbers.</param>
    /// <param name="index">0-based position (0 = [1..n], n!-1 = [n..1]).</param>
    public static int[] GetByIndex(int n, long index)
    {
        long total = CountPermutations(n);
        if (index < 0 || index >= total)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {total - 1}.");
        }

        var unusedNumbers = Enumerable.Range(1, n).ToList();
        var result = new int[n];
        long blockSize = total; // how many permutations share the same first number

        for (int position = 0; position < n; position++)
        {
            blockSize /= (n - position);
            int pick = (int)(index / blockSize);
            index %= blockSize;

            result[position] = unusedNumbers[pick];
            unusedNumbers.RemoveAt(pick);
        }

        return result;
    }

    private static void ValidateN(int n)
    {
        if (n < MinN || n > MaxN)
        {
            throw new ArgumentOutOfRangeException(nameof(n), $"n must be between {MinN} and {MaxN}.");
        }
    }
}
