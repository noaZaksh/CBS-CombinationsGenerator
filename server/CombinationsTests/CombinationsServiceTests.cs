using CombinationsCore.Services;
using CombinationsStorage;
using Microsoft.Extensions.Caching.Memory;

namespace CombinationsTests;

public class CombinationsServiceTests
{
    private static CombinationsService CreateService() =>
        new(new MemorySessionStore(new MemoryCache(new MemoryCacheOptions())));

    [Fact]
    public void Start_ReturnsTotalCountWithCommas()
    {
        var result = CreateService().Start(20);

        Assert.Equal("2432902008176640000", result.TotalCount);
        Assert.Equal("2,432,902,008,176,640,000", result.TotalCountText);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Start_RejectsNOutOfRange(int n)
    {
        Assert.Throws<ArgumentException>(() => CreateService().Start(n));
    }

    [Fact]
    public void GetNext_ReturnsCombinationsInOrderWithSerialNumbers_ThenNoMore()
    {
        var service = CreateService();
        var id = service.Start(3).SessionId;
        var expected = new[]
        {
            new[] { 1, 2, 3 }, new[] { 1, 3, 2 }, new[] { 2, 1, 3 },
            new[] { 2, 3, 1 }, new[] { 3, 1, 2 }, new[] { 3, 2, 1 },
        };

        for (int i = 0; i < expected.Length; i++)
        {
            var next = service.GetNext(id);
            Assert.True(next.HasCombination);
            Assert.Equal((i + 1).ToString(), next.Item!.Number);
            Assert.Equal(expected[i], next.Item.Combination);
        }

        var end = service.GetNext(id);
        Assert.False(end.HasCombination);
        Assert.Equal(CombinationsService.NoMoreCombinationsMessage, end.Message);
    }

    [Fact]
    public void GetNext_UnknownSession_Throws()
    {
        Assert.Throws<SessionNotFoundException>(() => CreateService().GetNext(Guid.NewGuid()));
    }

    [Fact]
    public void GetAll_StartsAfterTheCurrentCombination()
    {
        var service = CreateService();
        var id = service.Start(4).SessionId;
        service.GetNext(id);
        service.GetNext(id); // current = #2

        var page = service.GetAll(id, pageSize: 5, pageNumber: 1);

        Assert.Equal("3", page.Items[0].Number);
        Assert.Equal("7", page.Items[^1].Number);
        Assert.Equal("5", page.TotalPages); // 22 remaining / 5 per page = 5 pages
    }

    [Fact]
    public void GetAll_LastPageCanBeShorter()
    {
        var service = CreateService();
        var id = service.Start(3).SessionId;

        var page = service.GetAll(id, pageSize: 4, pageNumber: 2);

        Assert.Equal(2, page.Items.Count);
        Assert.Equal("5", page.Items[0].Number);
        Assert.Equal(new[] { 3, 2, 1 }, page.Items[^1].Combination);
    }

    [Fact]
    public void GetAll_ForN20_JumpsToTheLastPageInstantly()
    {
        var service = CreateService();
        var id = service.Start(20).SessionId;
        long totalPages = 2_432_902_008_176_640_000L / 10;

        var page = service.GetAll(id, pageSize: 10, pageNumber: totalPages);

        Assert.Equal("2432902008176640000", page.Items[^1].Number);
        Assert.Equal(Enumerable.Range(1, 20).Reverse().ToArray(), page.Items[^1].Combination);
    }

    [Fact]
    public void GetAll_WhenNothingIsLeft_ReturnsMessage()
    {
        var service = CreateService();
        var id = service.Start(1).SessionId;
        service.GetNext(id);

        var page = service.GetAll(id, pageSize: 10, pageNumber: 1);

        Assert.Empty(page.Items);
        Assert.Equal(CombinationsService.NoMoreCombinationsMessage, page.Message);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 0)]
    [InlineData(10, 2)]
    public void GetAll_RejectsBadPageInput(int pageSize, long pageNumber)
    {
        var service = CreateService();
        var id = service.Start(3).SessionId;

        Assert.Throws<ArgumentException>(() => service.GetAll(id, pageSize, pageNumber));
    }

    [Fact]
    public void Back_ShowsLastCombinationOfThePage_AndNextContinuesFromIt()
    {
        var service = CreateService();
        var id = service.Start(4).SessionId;
        service.GetNext(id); // current = #1

        service.GetAll(id, pageSize: 5, pageNumber: 1); // #2..#6
        service.GetAll(id, pageSize: 5, pageNumber: 2); // #7..#11

        var back = service.Back(id);
        Assert.Equal("11", back.Item!.Number);

        var next = service.GetNext(id);
        Assert.Equal("12", next.Item!.Number);

        // Opening the list again starts after #12
        var page = service.GetAll(id, pageSize: 5, pageNumber: 1);
        Assert.Equal("13", page.Items[0].Number);
    }

    [Fact]
    public void Back_WithoutShowingAnything_HasNoCombination()
    {
        var service = CreateService();
        var id = service.Start(3).SessionId;

        Assert.False(service.Back(id).HasCombination);
    }

    [Fact]
    public void DifferentUsers_DoNotAffectEachOther()
    {
        var service = CreateService();
        var user1 = service.Start(3).SessionId;
        var user2 = service.Start(5).SessionId;

        service.GetNext(user1);
        service.GetNext(user1);
        var user2First = service.GetNext(user2);

        Assert.Equal("1", user2First.Item!.Number);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, user2First.Item.Combination);
    }
}
