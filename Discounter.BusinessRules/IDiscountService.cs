namespace Discounter.Core;

public interface IDiscountService {
    Models.GenerateResponse GenerateCodes(Models.GenerateRequest request);
    Task<Models.UseCodeResponse> UseCodeAsync(Models.UseCodeRequest request);
}