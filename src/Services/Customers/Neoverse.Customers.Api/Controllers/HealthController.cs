using Microsoft.AspNetCore.Mvc;
using Neoverse.Customers.Infrastructure;
using StackExchange.Redis;

namespace Neoverse.Customers.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly CustomerDbContext _db;
    private readonly IConnectionMultiplexer _redis;

    public HealthController(CustomerDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbOk = await _db.Database.CanConnectAsync();
        var redisOk = await _redis.GetDatabase().PingAsync() != TimeSpan.Zero;
        return Ok(new { postgres = dbOk, redis = redisOk });
    }
}
