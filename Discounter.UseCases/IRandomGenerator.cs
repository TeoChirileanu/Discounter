namespace Discounter.Infra;

public interface IRandomGenerator {
    string GenerateCode(byte requestLength);
}