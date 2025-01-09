using Discounter.Infra;

namespace Discounter.Infrastructure;

public class FileSystemRandomGenerator : IRandomGenerator {
    public string GenerateCode(byte requestLength) {
        return Path.GetRandomFileName().Replace(".", "")[..requestLength];
    }
}