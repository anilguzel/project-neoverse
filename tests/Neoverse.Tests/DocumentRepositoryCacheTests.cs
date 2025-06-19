using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Neoverse.DocumentManagement.Domain.Entities;
using Neoverse.DocumentManagement.Infrastructure;
using StackExchange.Redis;
using Xunit;

public class DocumentRepositoryCacheTests
{
    private static DocumentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DocumentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DocumentDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldCacheDocument()
    {
        using var ctx = CreateContext();

        var redisDb = new Mock<IDatabase>();
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(m => m.GetDatabase(It.IsAny<int>(), null)).Returns(redisDb.Object);

        var repo = new DocumentRepository(ctx, redis.Object);
        var doc = new Document("title", Guid.NewGuid());

        await repo.AddAsync(doc);

        redisDb.Verify(db =>
            db.StringSetAsync($"document:{doc.Id}", It.IsAny<RedisValue>(), null, false, When.Always, CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFromCache_IfExists()
    {
        using var ctx = CreateContext();

        var doc = new Document("title", Guid.NewGuid());
        await ctx.Documents.AddAsync(doc);
        await ctx.SaveChangesAsync();

        var cachedJson = JsonSerializer.Serialize(doc);

        var redisDb = new Mock<IDatabase>();
        redisDb.Setup(db => db.StringGetAsync($"document:{doc.Id}", CommandFlags.None))
               .ReturnsAsync(cachedJson);

        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(m => m.GetDatabase(It.IsAny<int>(), null)).Returns(redisDb.Object);

        var repo = new DocumentRepository(ctx, redis.Object);
        var result = await repo.GetByIdAsync(doc.Id);

        Assert.NotNull(result);
        Assert.Equal(doc.Id, result!.Id);

        redisDb.Verify(db =>
            db.StringGetAsync($"document:{doc.Id}", CommandFlags.None), Times.Once);

        // Çünkü cache'den okuduk, tekrar yazmaması lazım
        redisDb.Verify(db =>
            db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), null, When.Always, CommandFlags.None),
            Times.Never);
    }
}
