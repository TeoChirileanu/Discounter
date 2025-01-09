using Discounter.Infra;
using Discounter.Infrastructure;
using FluentAssertions;

namespace Discounter.Tests;

public class RedisDiscountRepositoryTests {
    private readonly IDiscountRepository _repository = new RedisDiscountRepository();
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown() {
        await _repository.DisposeAsync();
    }
    
    [Test]
    public async Task IsCodeUniqueAsync_ShouldReturnTrue_WhenCodeIsUnique() {
        // Arrange
        const string code = "unique_code";

        // Act
        var isUnique = await _repository.IsCodeUniqueAsync(code);

        // Assert
        isUnique.Should().BeTrue();
    }
    
    [Test]
    public async Task IsCodeUniqueAsync_ShouldReturnFalse_WhenCodeExists() {
        // Arrange
        const string code = "existing_code";
        await _repository.SaveCodeAsync(code);

        // Act
        var isUnique = await _repository.IsCodeUniqueAsync(code);

        // Assert
        isUnique.Should().BeFalse();
    }
    
    [Test]
    public async Task GetCodeAsync_ShouldReturnNull_WhenCodeDoesNotExist() {
        // Arrange
        const string code = "nonexistent_code";

        // Act
        var discountCode = await _repository.GetCodeAsync(code);

        // Assert
        discountCode.Should().BeNull();
    }

    [Test]
    public async Task GetCodeAsync_ShouldReturnUsedCode_WhenCodeIsUsed() {
        // Arrange
        const string code = "used_code";
        await _repository.SaveCodeAsync(code);
        await _repository.MarkCodeAsUsedAsync(code);
        
        // Act
        var discountCode = await _repository.GetCodeAsync(code);
        
        // Assert
        discountCode.Should().NotBeNull();
        discountCode?.Code.Should().Be(code);
        discountCode?.IsUsed.Should().BeTrue();
    }

    [Test]
    public async Task GetCodeAsync_ShouldReturnUniqueCode_WhenCodeIsNotUsed() {
        // Arrange
        const string code = "unused_code";
        await _repository.SaveCodeAsync(code);
        
        // Act
        var discountCode = await _repository.GetCodeAsync(code);
        
        // Assert
        discountCode.Should().NotBeNull();
        discountCode?.Code.Should().Be(code);
        discountCode?.IsUsed.Should().BeFalse();
    }
}