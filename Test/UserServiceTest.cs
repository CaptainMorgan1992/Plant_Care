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
    
    [Test]
    public async Task IsOwnerAdminAsync_ReturnsTrue_IfUserIsAdmin()
    {
        // Arrange
        var ownerId = "abc";
        var users = new List<User>
        {
            new User { OwnerId = ownerId, IsAdmin = true }
        }.AsQueryable();

        var dbSetMock = new Mock<DbSet<User>>();
        dbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        dbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        dbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        dbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _dbMock.Setup(db => db.Users).Returns(dbSetMock.Object);

        // Act
        var result = await _userService.IsUserAdminAsync(ownerId);

        // Assert
        Assert.That(result, Is.True);
    }
    
    
}