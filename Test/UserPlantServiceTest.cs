using Auth0_Blazor.Data;
using Auth0_Blazor.Enums;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services;
using Auth0_Blazor.Services.IService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;

namespace Test;

[TestFixture]
public class UserPlantServiceTest
{
    private Mock<AuthenticationStateProvider> _authStateProvider = null!;
    private Mock<ILogger<UserPlantService>> _loggerMock = null!;
    private Mock<IUserService> _userService = null!;
    private ApplicationDbContext _db = null!;
    private Mock<UserPlantService> _userPlantServiceMock = null!;
    private UserPlantService _userPlantService = null!;

    [SetUp]
    public void Setup()
    {
        _authStateProvider = new Mock<AuthenticationStateProvider>();
        _loggerMock = new Mock<ILogger<UserPlantService>>();
        _userService = new Mock<IUserService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        _userPlantServiceMock = new Mock<UserPlantService>(_db, _userService.Object, _loggerMock.Object)
        {
            CallBase = true
        };
        _userPlantService = _userPlantServiceMock.Object;
    }

    [Test]
    public async Task AddPlantToUserHouseholdAsync_CallsAddPlantToUserWithExpectedArguments()
    {
        // Arrange
        const int plantId = 42;
        const string ownerId = "owner-1";
        const int userId = 123;

        _userService.Setup(x => x.SaveUserOnClick()).Returns(Task.CompletedTask);
        _userService.Setup(x => x.GetUserAuth0IdAsync()).ReturnsAsync(ownerId);
        _userService.Setup(x => x.IsValidUserByOwnerIdAsync(ownerId)).ReturnsAsync(userId);

        _userPlantServiceMock.Setup(x => x.AddPlantToUser(plantId, userId)).Returns(Task.CompletedTask).Verifiable();

        // Act
        await _userPlantService.AddPlantToUserHouseholdAsync(plantId);

        // Assert
        _userPlantServiceMock.Verify(x => x.AddPlantToUser(plantId, userId), Times.Once);
        _userService.Verify(x => x.SaveUserOnClick(), Times.Once);
        _userService.Verify(x => x.GetUserAuth0IdAsync(), Times.Once);
        _userService.Verify(x => x.IsValidUserByOwnerIdAsync(ownerId), Times.Once);
    }

    [Test]
    public async Task GetUserPlantsAsync_ReturnsUserPlantsList()
    {
        // Arrange
        const string fakeOwnerId = "owner-1";
        const int fakeUserId = 123;
        var fakeUserPlants = new List<UserPlant>
        {
            new UserPlant { PlantId = 1, UserId = 1 },
            new UserPlant { PlantId = 2, UserId = 2 }
        };

        _userService.Setup(x => x.GetUserAuth0IdAsync()).ReturnsAsync(fakeOwnerId);
        _userService.Setup(x => x.IsValidUserByOwnerIdAsync(fakeOwnerId)).ReturnsAsync(fakeUserId);
        _userPlantServiceMock.Setup(x => x.GetAllPlantsForUserById(fakeUserId)).ReturnsAsync(fakeUserPlants);

        // Act
        var result = await _userPlantService.GetUserPlantsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].PlantId, Is.EqualTo(1));
        Assert.That(result[1].PlantId, Is.EqualTo(2));

        _userService.Verify(x => x.GetUserAuth0IdAsync(), Times.Once);
        _userService.Verify(x => x.IsValidUserByOwnerIdAsync(fakeOwnerId), Times.Once);
        _userPlantServiceMock.Verify(x => x.GetAllPlantsForUserById(fakeUserId), Times.Once);
    }

    [Test]
    public async Task RemovePlantFromUserHouseholdAsync_RemovesUserPlant_WhenPlantExists()
    {
        // Arrange
        const int plantId = 42;
        const string ownerId = "owner-1";
        const int userId = 123;
        var userPlant = new UserPlant { PlantId = plantId, UserId = userId };

        _userService.Setup(x => x.GetUserAuth0IdAsync()).ReturnsAsync(ownerId);
        _userService.Setup(x => x.IsValidUserByOwnerIdAsync(ownerId)).ReturnsAsync(userId);

        // Mock DoesUserHavePlantAsync so that we don't have to set up the entire DB context
        _userPlantServiceMock.Setup(x => x.DoesUserHavePlantAsync(plantId, userId))
            .ReturnsAsync(userPlant);

        // Make sure that UserPlant is actually in the in-memory database
        _db.UserPlants.Add(userPlant);
        await _db.SaveChangesAsync();

        // Act
        await _userPlantService.RemovePlantFromUserHouseholdAsync(plantId);

        // Assert
        var fromDb = await _db.UserPlants
            .FirstOrDefaultAsync(up => up.PlantId == plantId && up.UserId == userId);
        Assert.That(fromDb, Is.Null);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => state.ToString()!.Contains("has been deleted from user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _userService.Verify(x => x.GetUserAuth0IdAsync(), Times.Once);
        _userService.Verify(x => x.IsValidUserByOwnerIdAsync(ownerId), Times.Once);
        _userPlantServiceMock.Verify(x => x.DoesUserHavePlantAsync(plantId, userId), Times.Once);
    }
    
    [Test]
    public async Task GetTop6UserPlantsAsync_Returns6Plants_WhenMoreThan6Exist()
    {
        // Arrange
        // 1. Mock topPlantIds
        var topPlantIds = new List<int> { 1, 2, 3, 4, 5, 6 };

        // 2. Mock topUserPlants (en per PlantId)
        var topUserPlants = topPlantIds
            .Select(id => new UserPlant { PlantId = id, UserId = 100 + id })
            .ToList();

        // 3. Setup mocks
        _userPlantServiceMock.Setup(x => x.FetchTopUserPlantIdsAsync()).ReturnsAsync(topPlantIds);
        _userPlantServiceMock.Setup(x => x.FetchTopUserPlantEntriesAsync(topPlantIds)).ReturnsAsync(topUserPlants);

        // 4. Om <6: mocka övriga
        // (I detta test, vi har redan 6, så dessa metoder ska inte anropas — men vi kan ändå lägga in en Verify om du vill)

        // Act
        var result = await _userPlantService.GetTop6UserPlantsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6));
        CollectionAssert.AreEquivalent(topPlantIds, result.Select(up => up.PlantId));

        _userPlantServiceMock.Verify(x => x.FetchTopUserPlantIdsAsync(), Times.Once);
        _userPlantServiceMock.Verify(x => x.FetchTopUserPlantEntriesAsync(topPlantIds), Times.Once);
        _userPlantServiceMock.Verify(x => x.FetchRemainingPlantsAsync(It.IsAny<HashSet<int>>()), Times.Never);
        _userPlantServiceMock.Verify(x => x.RandomizeRemainingPlantsAsync(It.IsAny<List<Plant>>(), It.IsAny<List<UserPlant>>()), Times.Never);
        _userPlantServiceMock.Verify(x => x.CreateNewListOfUserPlants(It.IsAny<List<UserPlant>>(), It.IsAny<List<Plant>>()), Times.Never);
    }
    
    [Test]
    public async Task GetUsersWithPlantsGroupedByWaterFrequencyAsync_ReturnsCorrectGrouping()
    {
        // Arrange
        var user1 = new User { Id = 1, OwnerId = "owner1", Name = "Alice" }; 
        var user2 = new User { Id = 2, OwnerId = "owner2", Name = "Bob" };
        _db.Users.AddRange(user1, user2);
        await _db.SaveChangesAsync();

        var userPlantsForUser1 = new List<UserPlant>
        {
            new UserPlant
            {
                Id = 1,
                UserId = 1,
                PlantId = 100,
                Plant = new Plant { Id = 101, Name = "Rose", WaterFrequency = WaterFrequency.High, Description = "A red rose", Origin = "Origin1", ImageUrl = "https://example.com/rose.jpg" }
            },
            new UserPlant
            {
                Id = 2,
                UserId = 1,
                PlantId = 101,
                Plant = new Plant { Id = 102, Name = "Cactus", WaterFrequency = WaterFrequency.Normal, Description = "A small cactus", Origin = "Origin2", ImageUrl = "https://example.com/rose2.jpg" }
            }
        };
        var groupedPlantsUser1 = new Dictionary<WaterFrequency, List<Plant>>
        {
            { WaterFrequency.High, new List<Plant> { userPlantsForUser1[0].Plant } },
            { WaterFrequency.Normal, new List<Plant> { userPlantsForUser1[1].Plant } }
        };

        var userPlantsForUser2 = new List<UserPlant>
        {
            new UserPlant
            {
                Id = 3,
                UserId = 2,
                PlantId = 201,
                Plant = new Plant { Id = 201, Name = "Fern", WaterFrequency = WaterFrequency.High, Description = "A leafy green", ImageUrl = "https://example.com/fern2.jpg", Origin = "FernCounty"}
            }
        };
        var groupedPlantsUser2 = new Dictionary<WaterFrequency, List<Plant>>
        {
            { WaterFrequency.High, new List<Plant> { userPlantsForUser2[0].Plant } }
        };

        _userPlantServiceMock.Setup(x => x.GetAllPlantsForUserById(1)).ReturnsAsync(userPlantsForUser1);
        _userPlantServiceMock.Setup(x => x.GetAllPlantsForUserById(2)).ReturnsAsync(userPlantsForUser2);

        _userPlantServiceMock.Setup(x => x.GroupPlantsByWateringNeedsAndReturnDictionary(userPlantsForUser1))
            .Returns(groupedPlantsUser1);
        _userPlantServiceMock.Setup(x => x.GroupPlantsByWateringNeedsAndReturnDictionary(userPlantsForUser2))
            .Returns(groupedPlantsUser2);

        // Act
        var result = await _userPlantService.GetUsersWithPlantsGroupedByWaterFrequencyAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ContainsKey(WaterFrequency.High));
        Assert.That(result.ContainsKey(WaterFrequency.Normal));

        var highList = result[WaterFrequency.High];
        Assert.That(highList.Count, Is.EqualTo(2));
        Assert.That(highList.Any(t => t.user.Name == "Alice" && t.plants.Any(p => p.Name == "Rose")));
        Assert.That(highList.Any(t => t.user.Name == "Bob" && t.plants.Any(p => p.Name == "Fern")));

        var normalList = result[WaterFrequency.Normal];
        Assert.That(normalList.Count, Is.EqualTo(1));
        Assert.That(normalList[0].user.Name, Is.EqualTo("Alice"));
        Assert.That(normalList[0].plants.Any(p => p.Name == "Cactus"));

        _userPlantServiceMock.Verify(x => x.GetAllPlantsForUserById(1), Times.Once);
        _userPlantServiceMock.Verify(x => x.GetAllPlantsForUserById(2), Times.Once);
        _userPlantServiceMock.Verify(x => x.GroupPlantsByWateringNeedsAndReturnDictionary(It.IsAny<List<UserPlant>>()), Times.Exactly(2));
    }
    
    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();
    }
}