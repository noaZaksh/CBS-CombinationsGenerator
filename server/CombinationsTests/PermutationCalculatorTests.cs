using CombinationsCore.Algorithms;

namespace CombinationsTests;

public class PermutationCalculatorTests
{
    [Theory]
    [InlineData(1, 1L)]
    [InlineData(3, 6L)]
    [InlineData(10, 3_628_800L)]
    [InlineData(20, 2_432_902_008_176_640_000L)]
    public void CountPermutations_ReturnsFactorial(int n, long expected)
    {
        Assert.Equal(expected, PermutationCalculator.CountPermutations(n));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void CountPermutations_RejectsNOutOfRange(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PermutationCalculator.CountPermutations(n));
    }

    [Fact]
    public void TryMoveToNext_ForN3_GivesTheOrderFromTheAssignment()
    {
        var expected = new[]
        {
            new[] { 1, 2, 3 }, new[] { 1, 3, 2 }, new[] { 2, 1, 3 },
            new[] { 2, 3, 1 }, new[] { 3, 1, 2 }, new[] { 3, 2, 1 },
        };

        var current = PermutationCalculator.GetFirst(3);
        foreach (var combination in expected)
        {
            Assert.Equal(combination, current);
            PermutationCalculator.TryMoveToNext(current);
        }
    }

    [Fact]
    public void TryMoveToNext_ReturnsFalseOnTheLastPermutation()
    {
        var last = new[] { 3, 2, 1 };
        Assert.False(PermutationCalculator.TryMoveToNext(last));
    }

    [Fact]
    public void TryMoveToNext_ForN1_HasOnlyOnePermutation()
    {
        var current = PermutationCalculator.GetFirst(1);
        Assert.False(PermutationCalculator.TryMoveToNext(current));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void GetByIndex_MatchesWalkingWithNextPermutation(int n)
    {
        // Both algorithms must agree on every single position.
        var current = PermutationCalculator.GetFirst(n);
        long total = PermutationCalculator.CountPermutations(n);

        for (long index = 0; index < total; index++)
        {
            Assert.Equal(current, PermutationCalculator.GetByIndex(n, index));
            PermutationCalculator.TryMoveToNext(current);
        }
    }

    [Fact]
    public void GetByIndex_ForN20_ReturnsFirstAndLastInstantly()
    {
        long total = PermutationCalculator.CountPermutations(20);

        Assert.Equal(Enumerable.Range(1, 20).ToArray(), PermutationCalculator.GetByIndex(20, 0));
        Assert.Equal(Enumerable.Range(1, 20).Reverse().ToArray(), PermutationCalculator.GetByIndex(20, total - 1));
    }

    [Fact]
    public void GetByIndex_RejectsIndexOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PermutationCalculator.GetByIndex(3, 6));
        Assert.Throws<ArgumentOutOfRangeException>(() => PermutationCalculator.GetByIndex(3, -1));
    }
}
