namespace Discounter.Core;

public static class Models {
    public record GenerateRequest(ushort Count, byte Length);

    public record GenerateResponse(bool Result, IAsyncEnumerable<string>? GeneratedCodes, string? ErrorMessage);
    
    
    public record UseCodeRequest(string Code);
    
    public record UseCodeResponse(UseCodeResult Result, string Message);
    public enum UseCodeResult : byte {
        Success = 0,
        NotFound = 1,
        AlreadyUsed = 2,
        InvalidFormat = 3,
        FailedToUse = 4,
    }

    public record DiscountCode(string Code, bool IsUsed);
}