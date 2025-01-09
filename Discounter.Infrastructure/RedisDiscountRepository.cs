using Discounter.Core;
using Discounter.Infra;
using StackExchange.Redis;

namespace Discounter.Infrastructure;

public class RedisDiscountRepository : IDiscountRepository {
    private readonly ConnectionMultiplexer _connection = ConnectionMultiplexer.Connect("localhost:6379");
    private readonly IDatabaseAsync _database;
    private const string DiscountCodesKey = "discount_codes";
    private const string UsedDiscountCodesKey = "used_discount_codes";

    public RedisDiscountRepository() {
        _database = _connection.GetDatabase();
    }
    
    public async Task<bool> IsCodeUniqueAsync(string code) {
        try
        {
            return !await _database.SetContainsAsync(DiscountCodesKey, code);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Models.DiscountCode?> GetCodeAsync(string code) {
        try
        {
            if (!await _database.SetContainsAsync(DiscountCodesKey, code)) return null;
            var isUsed = await _database.SetContainsAsync(UsedDiscountCodesKey, code);
            return new Models.DiscountCode(code, isUsed);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> MarkCodeAsUsedAsync(string code) {
        try {
            if (!await _database.SetContainsAsync(DiscountCodesKey, code)) return false;
            await _database.SetAddAsync(UsedDiscountCodesKey, code);
            return true;
        }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SaveCodeAsync(string code) {
        try
        {
            await _database.SetAddAsync(DiscountCodesKey, code);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }

    public async ValueTask DisposeAsync() {
        GC.SuppressFinalize(this);
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }
}