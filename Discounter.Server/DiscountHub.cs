using Discounter.Core;
using Microsoft.AspNetCore.SignalR;

namespace Discounter.Server;

public class DiscountHub(IDiscountService service) : Hub {
    public IAsyncEnumerable<string> GenerateCodes(Models.GenerateRequest request) {
        var response = service.GenerateCodes(request);
        if (!response.Result) Clients.Caller.SendAsync("OnError", response.ErrorMessage);
        return response.GeneratedCodes!;
    }
    
    public async Task<Models.UseCodeResponse> UseCode(Models.UseCodeRequest request) {
        return await service.UseCodeAsync(request);
    }
}