using Discounter.Core;
using Discounter.Infra;
using Discounter.Infrastructure;
using FluentAssertions;
using NSubstitute;

namespace Discounter.Tests;

public class DiscounterServiceTests {
    private readonly IDiscountRepository _repository = Substitute.For<IDiscountRepository>();
    private readonly IRandomGenerator _generator = new FileSystemRandomGenerator();
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown() {
        await _repository.DisposeAsync();
    }

    #region GenerateCodes
    
    [TestCase((ushort)1, 6)]
    [TestCase((ushort)1, 9)]
    [TestCase((ushort)1, 0)]
    [TestCase((ushort)0, 7)]
    [TestCase((ushort)2001, 8)]
    public void GenerateCodes_ShouldReturnError_WhenCalledWithInvalidParameters(ushort count, byte length) {
        // Arrange
        IDiscounterService service = new DiscounterService(_repository, _generator);

        // Act
        var generateResponse = service.GenerateCodes(new Models.GenerateRequest(count, length));

        // Assert
        generateResponse.Result.Should().BeFalse();
        generateResponse.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [TestCase((ushort)1, 7)]
    public async Task GenerateCodes_ShouldGenerateOneCode_WhenCalledWithValidParameters(ushort count, byte length) {
        // Arrange
        _repository.IsCodeUniqueAsync(Arg.Any<string>()).ReturnsForAnyArgs(true);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var generateResponse = service.GenerateCodes(new Models.GenerateRequest(count, length));
        
        // Assert
        generateResponse.Result.Should().BeTrue();
        generateResponse.ErrorMessage.Should().BeNull();
        generateResponse.GeneratedCodes.Should().NotBeNull();

        var codesCount = await generateResponse.GeneratedCodes!.CountAsync();
        codesCount.Should().Be(count);
    }
    
    [TestCase((ushort)2, 7)]
    public async Task GenerateCodes_ShouldReturnError_WhenGenerationFails(ushort count, byte length) {
        // Arrange
        _repository.IsCodeUniqueAsync(Arg.Any<string>()).ReturnsForAnyArgs(false);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var generateResponse = service.GenerateCodes(new Models.GenerateRequest(count, length));
        
        // Assert
        generateResponse.Result.Should().BeTrue();
        generateResponse.ErrorMessage.Should().BeNull();
        generateResponse.GeneratedCodes.Should().NotBeNull();

        Func<Task> act = async () => await generateResponse.GeneratedCodes!.CountAsync();
        await act.Should().ThrowAsync<Exception>();
    }

    [TestCase((ushort)2000, 7)]
    public async Task GenerateCodes_ShouldGenerateMaxCode_WhenCalledWithValidParameters(ushort count, byte length) {
        // Arrange
        _repository.IsCodeUniqueAsync(Arg.Any<string>()).ReturnsForAnyArgs(true);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var generateResponse = service.GenerateCodes(new Models.GenerateRequest(count, length));
        
        // Assert
        generateResponse.Result.Should().BeTrue();
        generateResponse.ErrorMessage.Should().BeNull();
        generateResponse.GeneratedCodes.Should().NotBeNull();

        var codesCount = await generateResponse.GeneratedCodes!.CountAsync();
        codesCount.Should().Be(count);
    }
    
    #endregion
    
    #region UseCode
    
    [TestCase("")]
    [TestCase("123456")]
    [TestCase("123456789")]
    public async Task UseCodeAsync_ShouldReturnError_WhenCalledWithInvalidCode(string code) {
        // Arrange
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var useCodeResponse = await service.UseCodeAsync(new Models.UseCodeRequest(code));
        
        // Assert
        useCodeResponse.Result.Should().Be(Models.UseCodeResult.InvalidFormat);
        useCodeResponse.Message.Should().NotBeNullOrWhiteSpace();
    }
    
    [Test]
    public async Task UseCodeAsync_ShouldReturnError_WhenCodeNotFound() {
        // Arrange
        _repository.GetCodeAsync(Arg.Any<string>()).ReturnsForAnyArgs((Models.DiscountCode)null);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var useCodeResponse = await service.UseCodeAsync(new Models.UseCodeRequest("1234567"));
        
        // Assert
        useCodeResponse.Result.Should().Be(Models.UseCodeResult.NotFound);
        useCodeResponse.Message.Should().NotBeNullOrWhiteSpace();
    }
    
    [Test]
    public async Task UseCodeAsync_ShouldReturnError_WhenCodeAlreadyUsed() {
        // Arrange
        var discountCode = new Models.DiscountCode("1234567", true);
        _repository.GetCodeAsync(Arg.Any<string>()).ReturnsForAnyArgs(discountCode);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var useCodeResponse = await service.UseCodeAsync(new Models.UseCodeRequest("1234567"));
        
        // Assert
        useCodeResponse.Result.Should().Be(Models.UseCodeResult.AlreadyUsed);
        useCodeResponse.Message.Should().NotBeNullOrWhiteSpace();
    }
    
    [Test]
    public async Task UseCodeAsync_ShouldReturnError_WhenFailedToUseCode() {
        // Arrange
        var discountCode = new Models.DiscountCode("1234567", false);
        _repository.GetCodeAsync(Arg.Any<string>()).ReturnsForAnyArgs(discountCode);
        _repository.MarkCodeAsUsedAsync(Arg.Any<string>()).ReturnsForAnyArgs(false);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var useCodeResponse = await service.UseCodeAsync(new Models.UseCodeRequest("1234567"));
        
        // Assert
        useCodeResponse.Result.Should().Be(Models.UseCodeResult.FailedToUse);
        useCodeResponse.Message.Should().NotBeNullOrWhiteSpace();
    }
    
    [Test]
    public async Task UseCodeAsync_ShouldReturnSuccess_WhenCodeUsed() {
        // Arrange
        var discountCode = new Models.DiscountCode("1234567", false);
        _repository.GetCodeAsync(Arg.Any<string>()).ReturnsForAnyArgs(discountCode);
        _repository.MarkCodeAsUsedAsync(Arg.Any<string>()).ReturnsForAnyArgs(true);
        IDiscounterService service = new DiscounterService(_repository, _generator);
        
        // Act
        var useCodeResponse = await service.UseCodeAsync(new Models.UseCodeRequest("1234567"));
        
        // Assert
        useCodeResponse.Result.Should().Be(Models.UseCodeResult.Success);
        useCodeResponse.Message.Should().NotBeNullOrWhiteSpace();
    }
    
    #endregion
}