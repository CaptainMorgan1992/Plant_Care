using System.Security.Claims;
using System.Threading.Tasks;
using Auth0_Blazor.Data;
using Auth0_Blazor.Models;
using Auth0_Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

[TestFixture]
public class UserServiceTests
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
    public async Task GetUserAuth0IdAsync_ReturnsUserId_IfClaimExists()
    {
        // Arrange: Mocka en användare med NameIdentifier
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "auth0|123")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var authState = new AuthenticationState(principal);

        _authStateProviderMock
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _userService.GetUserAuth0IdAsync();

        // Assert
        Assert.That(result, Is.EqualTo("auth0|123"));
    }

    [Test]
    public async Task GetUserAuth0IdAsync_ReturnsNull_IfClaimMissing()
    {
        // Arrange: Mocka en användare utan NameIdentifier
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = new AuthenticationState(principal);

        _authStateProviderMock
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        // Act
        var result = await _userService.GetUserAuth0IdAsync();

        // Assert
        Assert.That(result, Is.Null);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "No userId found. User details will not be saved."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Test]
    public void DoesUserIdHaveValue_ReturnsUserId_If_UserId_HasValue()
    {
        var input = "auth0|123";
        var result = _userService.DoesUserIdHaveValue(input);
        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void DoesUserIdHaveValue_ReturnsNull_If_UserId_IsNullOrWhiteSpace()
    {
        var input = " ";
        var result = _userService.DoesUserIdHaveValue(input);
        Assert.That(result, Is.Null);
    }
    
    /* This test is creating a new In-memory database and populates it with mocked user-objects.
     * Options is a common name for the DbContextOptions<ApplicationDbContext> object. This is also
     * a configuration-object. A randomized guid is created as a name for the DB so that no tests will collide with
     * one another.
     * All dependencies needs to be passed into userService in order to create an instance of it.
     */
    
    [Test]
    public async Task IsOwnerAdminAsync_ReturnsTrue_IfUserIsAdmin()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        var dbFactory = new TestDbContextFactory(dbContext);
        
        dbContext.Users.Add(new User { OwnerId = "admin-id", IsAdmin = true, Name = "Admin User" });
        dbContext.Users.Add(new User { OwnerId = "user-id", IsAdmin = false, Name = "Regular User" });
        await dbContext.SaveChangesAsync();
        
        var userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, dbFactory);

        var isAdmin = await userService.IsUserAdminAsync("admin-id");
        Assert.That(isAdmin, Is.True);

        var isUserAdmin = await userService.IsUserAdminAsync("user-id");
        Assert.That(isUserAdmin, Is.False);
    }

    [Test]
    public async Task GetUserIdByOwnerIdAsync_ReturnsUserId_IfUserExists()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        var dbFactory = new TestDbContextFactory(dbContext);
        
        dbContext.Users.Add(new User { Id = 1, OwnerId = "owner-1", Name = "Owner One" });
        dbContext.Users.Add(new User { Id = 2, OwnerId = "owner-2", Name = "Owner Two" });
        await dbContext.SaveChangesAsync();
        
        var userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, dbFactory);
        
        var userId = await userService.GetUserIdByOwnerIdAsync("owner-1");
        Assert.That(userId, Is.EqualTo(1));
    }
    
    /*
     * Created a claim where we extract the Name property from the identity.
     * A new user identity is then created with the claim and a new authentication state.
     * Then the object authstate is populated with the mocked authentication state.
     */
    [Test]
    public async Task GetCurrentUserNameAsync_ReturnsUserName_IfExists()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var authState = new AuthenticationState(principal);

        _authStateProviderMock
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var userName = await _userService.FetchCurrentUserNameAsync();
        Assert.That(userName, Is.EqualTo("Test User"));
    }
    
    [Test]
    public async Task SaveUserOnClickAsync_SavesUser_WhenUserIdAndNameAreValid()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        var dbFactory = new TestDbContextFactory(dbContext);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "auth0|123"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var authState = new AuthenticationState(principal);

        _authStateProviderMock
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        var userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, dbFactory);
        
        await userService.SaveUserOnClick();

        var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.OwnerId == "auth0|123");
        Assert.That(savedUser, Is.Not.Null);
        Assert.That("Test User",Is.EqualTo(savedUser?.Name));
    }
    
    [Test]
    public async Task DoesUserExist_ReturnsTrue_IfUserExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_UserExists")
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        dbContext.Users.Add(new User { OwnerId = "test-owner", Id = 123, Name = "Test User" });
        dbContext.Users.Add(new User { OwnerId = "other-owner", Id = 456, Name = "Other User" });
        await dbContext.SaveChangesAsync();

        // Act
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.OwnerId == "test-owner" && u.Id == 123);

        // Assert
        Assert.That(user, Is.Not.Null);
        Assert.That(user?.OwnerId, Is.EqualTo("test-owner"));
        Assert.That(user?.Id, Is.EqualTo(123));
    }
    
    [Test]
    public void IsUserIdNullOrWhiteSpace_ReturnsFalse_IfUserIdIsNullOrWhiteSpace()
    {
        var resultForEmptyString = _userService.IsUserIdNullOrWhiteSpace(" ");
        Assert.That(resultForEmptyString, Is.False);
        
        var resultForValidString = _userService.IsUserIdNullOrWhiteSpace("test");
        Assert.That(resultForValidString, Is.True);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString() == "No userId found. User details will not be saved."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Test]
    public async Task SaveUserDetailsToDb_AddsUserToDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        var dbFactory = new TestDbContextFactory(dbContext);
        
        var userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, dbFactory);
        
        await userService.SaveUserDetailsToDb("auth0|456", "New User");

        var savedUser = await dbContext.Users.FirstOrDefaultAsync(u => u.OwnerId == "auth0|456");
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser?.Name, Is.EqualTo("New User"));
    }
    
    [Test]
    public void DoesOwnerIdHaveStringValue_ThrowsException_IfOwnerIdIsNull()
    {
        const string ownerId = "OwnerIdTest";

        Assert.DoesNotThrow(() => _userService.ValidateOwnerId(ownerId));
        Assert.Throws<ArgumentNullException>(() => _userService.ValidateOwnerId(null));
    }
    
    [Test]
    public void DoesUserIdHaveIntValue_ThrowsException_IfUserIdIsNull()
    {
        int? userIdWithValue = 1;
        int? userIdWithoutValue = null;

        Assert.DoesNotThrow(() => _userService.DoesUserIdHaveIntValue(userIdWithValue));
        Assert.Throws<ArgumentNullException>(() => _userService.DoesUserIdHaveIntValue(userIdWithoutValue));
    }
    
    [Test]
    public async Task IsValidUserByOwnerIdAsync_ReturnsUserId_IfUserExists()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        var dbFactory = new TestDbContextFactory(dbContext);
        
        dbContext.Users.Add(new User { Id = 1, OwnerId = "owner-1", Name = "Owner One" });
        dbContext.Users.Add(new User { Id = 2, OwnerId = "owner-2", Name = "Owner Two" });
        await dbContext.SaveChangesAsync();
        
        var userService = new UserService(_authStateProviderMock.Object, _loggerMock.Object, dbFactory);
        
        var userId = await userService.IsValidUserByOwnerIdAsync("owner-1");
        Assert.That(userId, Is.EqualTo(1));
        
        var nonExistentUserId = await userService.IsValidUserByOwnerIdAsync("non-existent-owner");
        Assert.That(nonExistentUserId, Is.Null);
    }
    
    
}