using Auth0_Blazor.Services;

namespace Test;

using NUnit.Framework;

[TestFixture]
public class UtilityServiceTests
{
    private UtilityService _utilityService = null!;

    [SetUp]
    public void SetUp()
    {
        _utilityService = new UtilityService();
    }

    [Test]
    public void TruncateText_TextLongerThanMaxLength_ShouldTruncateAndAddEllipsis()
    {
        // Arrange
        const string text = "Detta är en väldigt lång text.";
        const int maxLength = 10;

        // Act
        var result = _utilityService.TruncateText(text, maxLength);

        // Assert
        Assert.That(result, Is.EqualTo("Detta är e..."));
    }

    [Test]
    public void TruncateText_TextShorterThanMaxLength_ShouldReturnOriginalText()
    {
        // Arrange
        const string text = "Kort text";
        const int maxLength = 20;

        // Act
        var result = _utilityService.TruncateText(text, maxLength);

        // Assert
        Assert.That(text, Is.EqualTo(result));
    }

    [Test]
    public void TruncateText_TextIsNull_ShouldReturnNull()
    {
        // Arrange
        string? text = null;
        const int maxLength = 10;

        // Act
        var result = _utilityService.TruncateText(text, maxLength);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TruncateText_TextIsEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        const string text = "";
        const int maxLength = 10;

        // Act
        var result = _utilityService.TruncateText(text, maxLength);

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }
}