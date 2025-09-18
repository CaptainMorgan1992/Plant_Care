using Auth0_Blazor.Models;
using Auth0_Blazor.Services;
using Auth0_Blazor.Services.IService;

namespace Test;

using NUnit.Framework;

[TestFixture]
public class NotificationServiceTests
{
    private NotificationService _notificationService = null!;
    private string _notifiedPlantName = null!;
    private string _notifiedOwnerId = null!;
    private bool _eventFired;

    [SetUp]
    public void SetUp()
    {
        _notificationService = new NotificationService();
        _eventFired = false;
        _notifiedPlantName = null;
        _notifiedOwnerId = null;

        _notificationService.OnWateringNotify += (plantName, ownerId) =>
        {
            _eventFired = true;
            _notifiedPlantName = plantName;
            _notifiedOwnerId = ownerId;
        };
    }

    [Test]
    public void ShowWateringNotification_ShouldInvokeOnWateringNotify_WithCorrectArguments()
    {
        // Arrange
        var user = new User { OwnerId = "owner-123" };
        const string plantName = "Fern";

        // Act
        _notificationService.ShowWateringNotification(user, plantName);

        // Assert
        Assert.That(_eventFired, "Event should have been fired");
        Assert.That("Fern", Is.EqualTo(_notifiedPlantName));
        Assert.That("owner-123", Is.EqualTo(_notifiedOwnerId));
    }
}