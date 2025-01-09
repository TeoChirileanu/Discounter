namespace Discounter.Core;

public interface IDiscounterService {
    Models.GenerateResponse GenerateCodes(Models.GenerateRequest request);
    Task<Models.UseCodeResponse> UseCodeAsync(Models.UseCodeRequest request);
}