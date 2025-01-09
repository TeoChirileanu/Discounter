using Discounter.Core;

namespace Discounter.Infra;

public class DiscounterService(IDiscountRepository repository, IRandomGenerator generator) : IDiscounterService {
    public Models.GenerateResponse GenerateCodes(Models.GenerateRequest request) {
        if (request.Length is < 7 or > 8) {
            return new Models.GenerateResponse(false, null, "Code length must be between 7 and 8 characters.");
        }

        if (request.Count is 0 or > 2000) {
            return new Models.GenerateResponse(false, null, "Count must be between 1 and 2000.");
        }

        var generatedCodes = GetGeneratedCodesAsync(request);
        return new Models.GenerateResponse(true, generatedCodes, null);
    }

    private IAsyncEnumerable<string> GetGeneratedCodesAsync(Models.GenerateRequest request, int maxAttempts = 100) {
        return AsyncEnumerable.Range(0, request.Count)
            .SelectAwait(async _ => {
                for (var attempt = 0; attempt < maxAttempts; attempt++) {
                    var code = generator.GenerateCode(request.Length);
                    if (await repository.IsCodeUniqueAsync(code)) {
                        await repository.SaveCodeAsync(code);
                        return code;
                    }
                }
                throw new Exception($"Failed to generate a unique code after {maxAttempts} attempts."); 
            });
    }

    public async Task<Models.UseCodeResponse> UseCodeAsync(Models.UseCodeRequest request) {
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length < 7 || request.Code.Length > 8) {
            return new Models.UseCodeResponse(Models.UseCodeResult.InvalidFormat, "Invalid code format.");
        }

        var code = request.Code.Trim();

        var discountCode = await repository.GetCodeAsync(code);

        if (discountCode == null) {
            return new Models.UseCodeResponse(Models.UseCodeResult.NotFound, "Code not found.");
        }

        if (discountCode.IsUsed) {
            return new Models.UseCodeResponse(Models.UseCodeResult.AlreadyUsed, "Code has already been used.");
        }

        try
        {
            var codeMarkedAsUsed = await repository.MarkCodeAsUsedAsync(code);
            if (!codeMarkedAsUsed) throw new Exception();
            return new Models.UseCodeResponse(Models.UseCodeResult.Success, "Code successfully used.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return new Models.UseCodeResponse(Models.UseCodeResult.FailedToUse, "Failed to use the code.");
        }
    }
}