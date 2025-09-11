using Auth0_Blazor;

namespace Test;

public class Tests
{
    [Test]
    public void PassingTest()
    {
        Assert.That(Calculator.Add(2,2), Is.EqualTo(4));
    }

    [Test]
    public void FailingTest()
    {
        Assert.That(Calculator.Add(2,3), Is.EqualTo(5));
    }
}