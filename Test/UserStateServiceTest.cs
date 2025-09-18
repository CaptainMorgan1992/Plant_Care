namespace Test;

using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Auth0_Blazor.Services;

[TestFixture]
public class UserStateServiceTests
{
    private UserStateService _userStateService = null!;
    private Mock<ILogger<UserStateService>> _mockLogger = null!;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<UserStateService>>();
        _userStateService = new UserStateService(_mockLogger.Object);
    }

    [Test]
    public void SetOwnerId_ShouldSetOwnerIdProperty()
    {
        // Arrange
        var ownerId = "user-123";

        // Act
        _userStateService.SetOwnerId(ownerId);

        // Assert
        Assert.That(ownerId, Is.EqualTo(_userStateService.OwnerId));
    }
}