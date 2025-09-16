using Auth0_Blazor.Data;
using Auth0_Blazor.Enums;
using Auth0_Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
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
}