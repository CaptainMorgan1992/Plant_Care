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
    private Mock<ILogger<UserPlantService>> _loggerMock = null!;
    private Mock<IUserService> _userService = null!;
    private ApplicationDbContext _db = null!;
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

        _userPlantService = new UserPlantService(_db, _userService.Object, _loggerMock.Object);
    }

    
    
}*/