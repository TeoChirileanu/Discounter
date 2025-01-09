using Discounter.Core;

namespace Discounter.Infra;

public interface IDiscountRepository : IAsyncDisposable {
    Task<bool> IsCodeUniqueAsync(string code);
    Task<Models.DiscountCode?> GetCodeAsync(string code);
    Task<bool> MarkCodeAsUsedAsync(string code);
    Task SaveCodeAsync(string code);
}