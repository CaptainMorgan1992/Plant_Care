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
    
    [TearDown]
    public void TearDown()
    {
        _db?.Dispose();
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

        // 2. Mock topUserPlants (one per PlantId)
        var topUserPlants = topPlantIds
            .Select(id => new UserPlant { PlantId = id, UserId = 100 + id })
            .ToList();

        // 3. Setup mocks
        _userPlantServiceMock.Setup(x => x.FetchTopUserPlantIdsAsync()).ReturnsAsync(topPlantIds);
        _userPlantServiceMock.Setup(x => x.FetchTopUserPlantEntriesAsync(topPlantIds)).ReturnsAsync(topUserPlants);

        // 4. If <6: mock the rest (not needed in this test)

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
    
    [Test]
    public void AddUserGroupedPlantsToDictionary_AddsUserAndPlantsToCorrectGroups()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Alice", OwnerId = "Owner1" };
        var plant1 = new Plant { Id = 10, Name = "Rose", WaterFrequency = WaterFrequency.High, Description = "A description", Origin = "Origin1", ImageUrl = "https://example.com/rose.jpg" };
        var plant2 = new Plant { Id = 11, Name = "Cactus", WaterFrequency = WaterFrequency.Normal, Description = "A description", Origin = "Origin1", ImageUrl = "https://example.com/rose.jpg" };

        var result = new Dictionary<WaterFrequency, List<(User, List<Plant>)>>();

        // Creates two groups of plants for the user
        var groupedPlants = new Dictionary<WaterFrequency, List<Plant>>
        {
            { WaterFrequency.High, new List<Plant> { plant1 } },
            { WaterFrequency.Normal, new List<Plant> { plant2 } }
        };

        var service = new UserPlantService(_db, _userService.Object, _loggerMock.Object); // eller din klass där metoden finns

        // Act
        var updated = service.AddUserGroupedPlantsToDictionary(result, groupedPlants, user);

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated.Count, Is.EqualTo(2));
        Assert.That(updated.ContainsKey(WaterFrequency.High));
        Assert.That(updated.ContainsKey(WaterFrequency.Normal));

        // Control that Alice and her plants are in the right groups
        var dailyGroup = updated[WaterFrequency.High];
        Assert.That(dailyGroup.Count, Is.EqualTo(1));
        Assert.That(dailyGroup[0].Item1.Name, Is.EqualTo("Alice"));
        Assert.That(dailyGroup[0].Item2.Count, Is.EqualTo(1));
        Assert.That(dailyGroup[0].Item2[0].Name, Is.EqualTo("Rose"));

        var weeklyGroup = updated[WaterFrequency.Normal];
        Assert.That(weeklyGroup.Count, Is.EqualTo(1));
        Assert.That(weeklyGroup[0].Item1.Name, Is.EqualTo("Alice"));
        Assert.That(weeklyGroup[0].Item2.Count, Is.EqualTo(1));
        Assert.That(weeklyGroup[0].Item2[0].Name, Is.EqualTo("Cactus"));
    }
    
    
    [Test]
    public async Task FetchTopUserPlantIdsAsync_Returns6MostPopularPlantIds()
    {
        // Arrange
        var userPlants = new List<UserPlant>
        {
            new UserPlant
            {
                Id = 1,
                UserId = 1,
                PlantId = 1,
                Plant = new Plant
                {
                    Id = 1,
                    Name = "Rose",
                    WaterFrequency = WaterFrequency.High,
                    Description = "A red rose",
                    Origin = "Origin1",
                    ImageUrl = "https://example.com/rose.jpg"
                }
            },
            new UserPlant
            {
                Id = 2,
                UserId = 2,
                PlantId = 2,
                Plant = new Plant
                {
                    Id = 2,
                    Name = "Cactus",
                    WaterFrequency = WaterFrequency.Normal,
                    Description = "A small cactus",
                    Origin = "Origin2",
                    ImageUrl = "https://example.com/cactus.jpg"
                }
            },
            new UserPlant
            {
                Id = 3,
                UserId = 1,
                PlantId = 3,
                Plant = new Plant
                {
                    Id = 3,
                    Name = "Fern",
                    WaterFrequency = WaterFrequency.High,
                    Description = "A green fern",
                    Origin = "Origin3",
                    ImageUrl = "https://example.com/fern.jpg"
                }
            },
            new UserPlant
            {
                Id = 4,
                UserId = 3,
                PlantId = 4,
                Plant = new Plant
                {
                    Id = 4,
                    Name = "Lily",
                    WaterFrequency = WaterFrequency.Normal,
                    Description = "A beautiful lily",
                    Origin = "Origin4",
                    ImageUrl = "https://example.com/lily.jpg"
                }
            },
            new UserPlant
            {
                Id = 5,
                UserId = 4,
                PlantId = 5,
                Plant = new Plant
                {
                    Id = 5,
                    Name = "Lily",
                    WaterFrequency = WaterFrequency.Normal,
                    Description = "A beautiful lily",
                    Origin = "Origin4",
                    ImageUrl = "https://example.com/lily.jpg"
                }
            },
            new UserPlant
            {
                Id = 6,
                UserId = 5,
                PlantId = 6,
                Plant = new Plant
                {
                    Id = 6,
                    Name = "Lily",
                    WaterFrequency = WaterFrequency.Normal,
                    Description = "A beautiful lily",
                    Origin = "Origin4",
                    ImageUrl = "https://example.com/lily.jpg"
                }
            }
        };
        _db.UserPlants.AddRange(userPlants);
        await _db.SaveChangesAsync();

        // Act
        var result = await _userPlantService.FetchTopUserPlantIdsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6));
        // Control that top 6 ids are correct
        var expected = new List<int> { 1, 2, 3, 4, 5, 6 };
        CollectionAssert.AreEqual(expected, result);
    }
    
    [Test]
    public async Task FetchTopUserPlantEntriesAsync_ReturnsFirstEntryOfEachPlantId()
    {
        // Arrange
        var plant1 = new Plant {
            Id = 1,
            Name = "Lily",
            WaterFrequency = WaterFrequency.Normal,
            Description = "A beautiful lily",
            Origin = "Origin1",
            ImageUrl = "https://example.com/lily.jpg"
        };
        var plant2 = new Plant {
            Id = 2,
            Name = "Rose",
            WaterFrequency = WaterFrequency.Normal,
            Description = "A beautiful Rose",
            Origin = "Origin2",
            ImageUrl = "https://example.com/rose.jpg"
        };
        var plant3 = new Plant {
            Id = 3,
            Name = "Fern",
            WaterFrequency = WaterFrequency.High,
            Description = "A beautiful fern",
            Origin = "Origin3",
            ImageUrl = "https://example.com/fern.jpg"
        };

        _db.Plants.AddRange(plant1, plant2, plant3);

        var userPlants = new List<UserPlant>
        {
            new UserPlant { Id = 10, UserId = 1, PlantId = 1, Plant = plant1 },
            new UserPlant { Id = 11, UserId = 2, PlantId = 1, Plant = plant1 }, // another user, same plant
            new UserPlant { Id = 12, UserId = 1, PlantId = 2, Plant = plant2 },
            new UserPlant { Id = 13, UserId = 3, PlantId = 3, Plant = plant3 }
        };
        _db.UserPlants.AddRange(userPlants);
        await _db.SaveChangesAsync();

        var topPlantIds = new List<int> { 1, 2 };

        // Act
        var result = await _userPlantService.FetchTopUserPlantEntriesAsync(topPlantIds);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(up => up.PlantId == 1));
        Assert.That(result.Any(up => up.PlantId == 2));
        // Should be the first userPlant for each plantId (by insertion order)
        Assert.That(result.Any(up => up.Id == 10));
        Assert.That(result.Any(up => up.Id == 12));
        // Should not include plantId 3
        Assert.That(result.All(up => topPlantIds.Contains(up.PlantId)), Is.True);
    }
    
    [Test]
    public async Task AddPlantToUser_AddsUserPlantToDatabase()
    {
        // Arrange
        const int plantId = 6;
        const int userId = 42;

        // (Optional) Ensure the database is empty at the start
        Assert.That(_db.UserPlants.Count(), Is.EqualTo(0));

        // Act
        await _userPlantService.AddPlantToUser(plantId, userId);

        // Assert
        var userPlant = await _db.UserPlants.FirstOrDefaultAsync(up => up.PlantId == plantId && up.UserId == userId);
        Assert.That(userPlant, Is.Not.Null);
        Assert.That(userPlant?.PlantId, Is.EqualTo(plantId));
        Assert.That(userPlant?.UserId, Is.EqualTo(userId));
    }
    
    [Test]
    public async Task GetAllPlantsForUserById_ReturnsUserPlantsWithPlantsIncluded()
    {
        // Arrange
        const int userId = 99;

        var plant1 = new Plant
        {
            Id = 1,
            Name = "Rose",
            WaterFrequency = WaterFrequency.High,
            Description = "A red rose",
            Origin = "Origin1",
            ImageUrl = "https://example.com/rose.jpg"
        };
        var plant2 = new Plant
        {
            Id = 2,
            Name = "Lily",
            WaterFrequency = WaterFrequency.Normal,
            Description = "A beautiful lily",
            Origin = "Origin2",
            ImageUrl = "https://example.com/lily.jpg"
        };

        // Lägger till Plant-objekten separat om din databas kräver det
        _db.Plants.AddRange(plant1, plant2);

        var userPlants = new List<UserPlant>
        {
            new UserPlant { Id = 10, UserId = userId, PlantId = 1, Plant = plant1 },
            new UserPlant { Id = 11, UserId = userId, PlantId = 2, Plant = plant2 },
            new UserPlant { Id = 12, UserId = 5, PlantId = 2, Plant = plant2 } // annan användare
        };
        _db.UserPlants.AddRange(userPlants);
        await _db.SaveChangesAsync();

        // Act
        var result = await _userPlantService.GetAllPlantsForUserById(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(up => up.UserId == userId), Is.True);
        Assert.That(result.Any(up => up.Plant?.Name == "Rose"));
        Assert.That(result.Any(up => up.Plant?.Name == "Lily"));
        Assert.That(result.All(up => up.Plant != null), Is.True); // Verifierar Include
    }
    
    [Test]
    public async Task DoesUserHavePlantAsync_ReturnsUserPlant_WhenUserHasPlant()
    {
        // Arrange
        var userId = 77;
        var plantId = 15;
        var userPlant = new UserPlant { Id = 1, UserId = userId, PlantId = plantId };

        _db.UserPlants.Add(userPlant);
        await _db.SaveChangesAsync();

        // Act
        var result = await _userPlantService.DoesUserHavePlantAsync(plantId, userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.UserId, Is.EqualTo(userId));
        Assert.That(result?.PlantId, Is.EqualTo(plantId));
    }

    [Test]
    public async Task DoesUserHavePlantAsync_ReturnsNull_AndLogs_WhenUserDoesNotHavePlant()
    {
        // Arrange
        const int userId = 99;
        const int plantId = 25;

        // Act
        var result = await _userPlantService.DoesUserHavePlantAsync(plantId, userId);

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task FetchRemainingPlantsAsync_ReturnsPlantsNotInExistingPlantIds()
    {
        // Arrange
        var plant1 = new Plant {
            Id = 1,
            Name = "Lily",
            WaterFrequency = WaterFrequency.Normal,
            Description = "A beautiful lily",
            Origin = "Origin1",
            ImageUrl = "https://example.com/lily.jpg"
        };
        var plant2 = new Plant {
            Id = 2,
            Name = "Rose",
            WaterFrequency = WaterFrequency.Normal,
            Description = "A beautiful Rose",
            Origin = "Origin2",
            ImageUrl = "https://example.com/rose.jpg"
        };
        var plant3 = new Plant {
            Id = 3,
            Name = "Fern",
            WaterFrequency = WaterFrequency.High,
            Description = "A beautiful fern",
            Origin = "Origin3",
            ImageUrl = "https://example.com/fern.jpg"
        };

        _db.Plants.AddRange(plant1, plant2, plant3);
        await _db.SaveChangesAsync();

        var existingPlantIds = new HashSet<int> { 1, 3 }; // Fern och Lily finns redan

        // Act
        var result = await _userPlantService.FetchRemainingPlantsAsync(existingPlantIds);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(2));
        Assert.That(result[0].Name, Is.EqualTo("Rose"));
    }
    
    [Test]
    public void RandomizeRemainingPlantsAsync_ReturnsCorrectNumberOfPlants()
    {
        // Arrange
        var remainingPlants = new List<Plant>
        {
            new Plant { Id = 1, Name = "Rose", WaterFrequency = WaterFrequency.Normal, Description = "A lovely rose", Origin = "Origin1", ImageUrl = "https://example.com/rose.jpg" },
            new Plant { Id = 2, Name = "Lily", WaterFrequency = WaterFrequency.Low, Description = "A fragrant lily", Origin = "Origin2", ImageUrl = "https://example.com/lily.jpg" },
            new Plant { Id = 3, Name = "Cactus", WaterFrequency = WaterFrequency.Low, Description = "A spiky cactus", Origin = "Origin3", ImageUrl = "https://example.com/cactus.jpg" },
            new Plant { Id = 4, Name = "Tulip", WaterFrequency = WaterFrequency.Normal, Description = "A spring tulip", Origin = "Origin4", ImageUrl = "https://example.com/tulip.jpg" },
            new Plant { Id = 5, Name = "Fern", WaterFrequency = WaterFrequency.High, Description = "A beautiful fern", Origin = "Origin5", ImageUrl = "https://example.com/fern.jpg" },
            new Plant { Id = 6, Name = "Palm", WaterFrequency = WaterFrequency.Normal, Description = "A tropical palm", Origin = "Origin6", ImageUrl = "https://example.com/palm.jpg" }
        };

        var topUserPlants = new List<UserPlant>
        {
            new UserPlant { Id = 101, PlantId = 10, UserId = 1},
            new UserPlant { Id = 102, PlantId = 11, UserId = 1}
        };

        // Act
        var result = _userPlantService.RandomizeRemainingPlantsAsync(remainingPlants, topUserPlants);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(6 - topUserPlants.Count)); // Ska bli 4
        Assert.That(result.All(p => remainingPlants.Select(rp => rp.Id).Contains(p.Id)), Is.True);
    }

    [Test]
    public void RandomizeRemainingPlantsAsync_ReturnsEmptyList_IfTopUserPlantsCountIsSixOrMore()
    {
        // Arrange
        var remainingPlants = new List<Plant>
        {
            new Plant { Id = 1, Name = "Rose", WaterFrequency = WaterFrequency.Normal, Description = "A lovely rose", Origin = "Origin1", ImageUrl = "https://example.com/rose.jpg" },
            new Plant { Id = 2, Name = "Lily", WaterFrequency = WaterFrequency.Low, Description = "A fragrant lily", Origin = "Origin2", ImageUrl = "https://example.com/lily.jpg" }
        };
        var topUserPlants = new List<UserPlant>
        {
            new UserPlant { Id = 101, PlantId = 10, UserId = 1 },
            new UserPlant { Id = 102, PlantId = 11, UserId = 2 },
            new UserPlant { Id = 103, PlantId = 12, UserId = 3 },
            new UserPlant { Id = 104, PlantId = 13, UserId = 4 },
            new UserPlant { Id = 105, PlantId = 14, UserId = 5 },
            new UserPlant { Id = 106, PlantId = 15, UserId = 6 }
        };

        // Act
        var result = _userPlantService.RandomizeRemainingPlantsAsync(remainingPlants, topUserPlants);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void RandomizeRemainingPlantsAsync_ReturnsAllRemainingPlants_IfLessThanNeeded()
    {
        // Arrange
        var remainingPlants = new List<Plant>
        {
            new Plant { Id = 1, Name = "Rose", WaterFrequency = WaterFrequency.Normal, Description = "A lovely rose", Origin = "Origin1", ImageUrl = "https://example.com/rose.jpg" },
            new Plant { Id = 2, Name = "Lily", WaterFrequency = WaterFrequency.Low, Description = "A fragrant lily", Origin = "Origin2", ImageUrl = "https://example.com/lily.jpg" }
        };
        var topUserPlants = new List<UserPlant>
        {
            new UserPlant { Id = 101, PlantId = 10, UserId = 1}
        };

        // Act
        var result = _userPlantService.RandomizeRemainingPlantsAsync(remainingPlants, topUserPlants);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2)); // 6-1=5 men vi har bara 2
        Assert.That(result.All(p => remainingPlants.Contains(p)), Is.True);
    }
    
    [Test]
    public void CreateNewListOfUserPlants_AddsRandomPlantsToTopUserPlants()
    {
        // Arrange
        var topUserPlants = new List<UserPlant>
        {
            new UserPlant
            {
                Id = 1,
                PlantId = 3,
                UserId = 42,
                Plant = new Plant
                {
                    Id = 3,
                    Name = "Fern",
                    WaterFrequency = WaterFrequency.High,
                    Description = "A beautiful fern",
                    Origin = "Origin3",
                    ImageUrl = "https://example.com/fern.jpg"
                }
            }
        };

        var randomPlants = new List<Plant>
        {
            new Plant
            {
                Id = 7,
                Name = "Begonia",
                WaterFrequency = WaterFrequency.Normal,
                Description = "A colorful begonia",
                Origin = "Origin7",
                ImageUrl = "https://example.com/begonia.jpg"
            },
            new Plant
            {
                Id = 8,
                Name = "Orchid",
                WaterFrequency = WaterFrequency.Low,
                Description = "An exotic orchid",
                Origin = "Origin8",
                ImageUrl = "https://example.com/orchid.jpg"
            }
        };

        // Act
        var result = _userPlantService.CreateNewListOfUserPlants(topUserPlants, randomPlants);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));

        // Control that the first objects are the original ones
        Assert.That(result[0].PlantId, Is.EqualTo(3));
        Assert.That(result[0].Plant?.Name, Is.EqualTo("Fern"));
        Assert.That(result[0].UserId, Is.EqualTo(42));

        // Control that the new plants are added 
        Assert.That(result[1].PlantId, Is.EqualTo(7));
        Assert.That(result[1].Plant?.Name, Is.EqualTo("Begonia"));
        Assert.That(result[1].UserId, Is.EqualTo(0));

        Assert.That(result[2].PlantId, Is.EqualTo(8));
        Assert.That(result[2].Plant?.Name, Is.EqualTo("Orchid"));
        Assert.That(result[2].UserId, Is.EqualTo(0));
    }
    
    [Test]
    public void GroupPlantsByWateringNeedsAndReturnDictionary_GroupsPlantsCorrectlyByWaterFrequency()
    {
        // Arrange
        var fern = new Plant
        {
            Id = 1,
            Name = "Fern",
            WaterFrequency = WaterFrequency.High,
            Description = "A beautiful fern",
            Origin = "Origin1",
            ImageUrl = "https://example.com/fern.jpg"
        };
        var cactus = new Plant
        {
            Id = 2,
            Name = "Cactus",
            WaterFrequency = WaterFrequency.Low,
            Description = "A spiky cactus",
            Origin = "Origin2",
            ImageUrl = "https://example.com/cactus.jpg"
        };
        var lily = new Plant
        {
            Id = 3,
            Name = "Lily",
            WaterFrequency = WaterFrequency.Normal,
            Description = "A fragrant lily",
            Origin = "Origin3",
            ImageUrl = "https://example.com/lily.jpg"
        };
        var palm = new Plant
        {
            Id = 4,
            Name = "Palm",
            WaterFrequency = WaterFrequency.High,
            Description = "A tropical palm",
            Origin = "Origin4",
            ImageUrl = "https://example.com/palm.jpg"
        };

        var userPlants = new List<UserPlant>
        {
            new UserPlant { Id = 10, PlantId = fern.Id, Plant = fern, UserId = 1},
            new UserPlant { Id = 11, PlantId = cactus.Id, Plant = cactus, UserId = 2 },
            new UserPlant { Id = 12, PlantId = lily.Id, Plant = lily, UserId = 3 },
            new UserPlant { Id = 13, PlantId = palm.Id, Plant = palm, UserId = 4 },
            new UserPlant { Id = 14, PlantId = 999, Plant = null, UserId = 5 } // ska ignoreras
        };

        // Act
        var result = _userPlantService.GroupPlantsByWateringNeedsAndReturnDictionary(userPlants);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.ContainsKey(WaterFrequency.High));
        Assert.That(result.ContainsKey(WaterFrequency.Normal));
        Assert.That(result.ContainsKey(WaterFrequency.Low));
        Assert.That(result[WaterFrequency.High].Count, Is.EqualTo(2));
        Assert.That(result[WaterFrequency.High].Any(p => p.Id == fern.Id));
        Assert.That(result[WaterFrequency.High].Any(p => p.Id == palm.Id));
        Assert.That(result[WaterFrequency.Normal].Count, Is.EqualTo(1));
        Assert.That(result[WaterFrequency.Normal][0].Id, Is.EqualTo(lily.Id));
        Assert.That(result[WaterFrequency.Low].Count, Is.EqualTo(1));
        Assert.That(result[WaterFrequency.Low][0].Id, Is.EqualTo(cactus.Id));
    }
}