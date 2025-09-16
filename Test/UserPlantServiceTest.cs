/*using Auth0_Blazor.Data;
using Auth0_Blazor.Services;
using Auth0_Blazor.Services.IService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test;

[TestFixture]
public class UserPlantServiceTest
{
    private Mock<AuthenticationStateProvider> _authStateProvider = null!;
    private Mock<ILogger<UserPlantService>> _logger = null!;
    private Mock<IUserService> _userService = null!;
    private ApplicationDbContext _db = null!;
    private UserPlantService _service = null!;

    [SetUp]
    public void Setup()
    {
        _authStateProvider = new Mock<AuthenticationStateProvider>();
        _logger = new Mock<ILogger<UserPlantService>>();
        _userService = new Mock<IUserService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        _service = new UserPlantService(_db, _userService.Object, _logger.Object);
    }

    
        [Test]
    public async Task AddPlantToUserHouseholdAsync_PlantNotAlreadyAdded_AddsPlant()
    {
        // Arrange
        int plantId = 1;
        string ownerId = "owner-123";
        int? userId = 42;

        _userService.Setup(x => x.SaveUserOnClick()).Returns(Task.CompletedTask);
        _userService.Setup(x => x.GetUserAuth0IdAsync()).ReturnsAsync(ownerId);
        _userService.Setup(x => x.GetUserIdByOwnerIdAsync(ownerId)).ReturnsAsync(userId);
        _userService.Setup(x => x.DoesUserIdHaveIntValue(userId));
        
        // Plant not already added
        var _service = new Mock<IUserService>(_userService.Object, _loggerMock.Object) { CallBase = true }.Object;
        Mock.Get(_service)
            .Setup(x => x.PlantAlreadyAdded(userId.Value, plantId))
            .ReturnsAsync(false);
        Mock.Get(_service)
            .Setup(x => x.AddPlantToUser(plantId, userId.Value))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddPlantToUserHouseholdAsync(plantId);

        // Assert
        Mock.Get(_service).Verify(x => x.AddPlantToUser(plantId, userId.Value), Times.Once);
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Never);
    }

    [Test]
    public async Task AddPlantToUserHouseholdAsync_PlantAlreadyAdded_DoesNotAddPlant_LogsInfo()
    {
        // Arrange
        int plantId = 1;
        string ownerId = "owner-123";
        int? userId = 42;

        _userServiceMock.Setup(x => x.SaveUserOnClick()).Returns(Task.CompletedTask);
        _userServiceMock.Setup(x => x.GetUserAuth0IdAsync()).ReturnsAsync(ownerId);
        _userServiceMock.Setup(x => x.GetUserIdByOwnerIdAsync(ownerId)).ReturnsAsync(userId);
        _userServiceMock.Setup(x => x.DoesUserIdHaveIntValue(userId));
        
        // Plant already added
        _service = new Mock<UserHouseholdService>(_userServiceMock.Object, _loggerMock.Object) { CallBase = true }.Object;
        Mock.Get(_service)
            .Setup(x => x.PlantAlreadyAdded(userId.Value, plantId))
            .ReturnsAsync(true);

        // Act
        await _service.AddPlantToUserHouseholdAsync(plantId);

        // Assert
        Mock.Get(_service).Verify(x => x.AddPlantToUser(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _loggerMock.Verify(
            x => x.LogInformation("PlantId {PlantId} is already connected to {UserId}.", plantId, userId.Value),
            Times.Once);
    }
}*/