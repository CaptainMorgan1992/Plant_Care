using Auth0_Blazor.Data;
using Auth0_Blazor.Enums;
using Auth0_Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test;


[TestFixture]
public class PlantServiceTest
{
    private Mock<AuthenticationStateProvider> _authStateProviderMock = null!;
    private Mock<ILogger<UserService>> _loggerMock = null!;
    private Mock<ApplicationDbContext> _dbMock = null!;
    private UserService _userService = null!;
    
    [SetUp]
    public void Setup()
    {
        _authStateProviderMock = new Mock<AuthenticationStateProvider>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, null!);
        _dbMock = new Mock<ApplicationDbContext>();
    }
    
    [Test]
    public async Task FetchAllPlantsFromDb_ShouldReturnPlants()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        var dbContext = new ApplicationDbContext(options);
        
        dbContext.Plants.Add(new Auth0_Blazor.Models.Plant
            {   Id = 1,
                Name = "Plant1", 
                Description = "Some description",
                ImageUrl = "https://example.com/plant1.jpg",
                Origin = "Origin1",
                WaterFrequency = WaterFrequency.Normal
            });
        
        dbContext.Plants.Add(new Auth0_Blazor.Models.Plant
            {   Id = 2,
                Name = "Plant2", 
                Description = "Some description2",
                ImageUrl = "https://example.com/plant2.jpg",
                Origin = "Origin2",
                WaterFrequency = WaterFrequency.High
            });
        
        dbContext.SaveChanges();
        
        var plantService = new PlantService(dbContext, _userService);
        var result = await plantService.GetAllPlantsAsync();
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetOnePlantById_ShouldReturnPlantObject_OrReturnExceptionIfNotFound()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        var dbContext = new ApplicationDbContext(options);
        
        dbContext.Plants.Add(new Auth0_Blazor.Models.Plant
        {   Id = 1,
            Name = "Plant1", 
            Description = "Some description",
            ImageUrl = "https://example.com/plant1.jpg",
            Origin = "Origin1",
            WaterFrequency = WaterFrequency.Normal
        });
        
        await dbContext.SaveChangesAsync();
        
        var plantService = new PlantService(dbContext, _userService);
        var result = await plantService.GetPlantByIdAsync(1);
        
        Assert.That(result.Name, Is.EqualTo("Plant1"));
        Assert.That(result.Description, Is.EqualTo("Some description"));
        Assert.That(result.ImageUrl, Is.EqualTo("https://example.com/plant1.jpg"));
        Assert.That(result.Origin, Is.EqualTo("Origin1"));
        Assert.That(result.WaterFrequency, Is.EqualTo(WaterFrequency.Normal));
        
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await plantService.GetPlantByIdAsync(999));
    }

    [Test]
    public async Task AddNewPlant_ShouldThrowException_IfUserIsNotAdmin()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        var userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, dbContext);
        var plantService = new PlantService(dbContext, userService);
        
        var newPlant = new Auth0_Blazor.Models.Plant
        {
            Name = "Plant1",
            Description = "Some description",
            ImageUrl = "https://example.com/plant1.jpg",
            Origin = "Origin1",
            WaterFrequency = WaterFrequency.Normal
        };
        
        var nonAdminUser = new Auth0_Blazor.Models.User
        {
            OwnerId = "non-admin-owner-id",
            IsAdmin = false,
            Name = "NonAdminUser",
        };
        
        var adminUser = new Auth0_Blazor.Models.User
        {
            OwnerId = "admin-owner-id",
            IsAdmin = true,
            Name = "AdminUser"
        };

        dbContext.Users.Add(nonAdminUser);
        dbContext.Users.Add(adminUser);
        
        await dbContext.SaveChangesAsync();
        
        var ownerIsNotAdmin = await userService.IsUserAdminAsync("non-admin-owner-id");
        Assert.That(ownerIsNotAdmin, Is.False);
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await plantService.AddNewPlantAsync(newPlant, "non-admin-owner-id"));
        
        var ownerIsAdmin = await userService.IsUserAdminAsync("admin-owner-id");
        Assert.That(ownerIsAdmin, Is.True);
        var result = await plantService.AddNewPlantAsync(newPlant, "admin-owner-id");
        Assert.That(result, Is.True);
        
    }

}