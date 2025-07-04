namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionArrayTests
{
    [TestMethod]
    public void PushFrontAddsToHighResolution()
    {
        var arr = new MyConsoleApp.CircularMultiResolutionArray<int>(1, 4, 2);
        arr.PushFront(1);
        Assert.AreEqual(1, arr[0, 0]);
        arr.PushFront(2);
        Assert.AreEqual(2, arr[0, 0]);
        Assert.AreEqual(1, arr[0, 1]);
    }

    [TestMethod]
    public void AveragesAreAddedToLowerResolution()
    {
        var arr = new MyConsoleApp.CircularMultiResolutionArray<int>(2, 3, 2);
        arr.PushFront(1);
        arr.PushFront(3); // triggers average 2
        Assert.AreEqual(2, arr[1, 0]);

        arr.PushFront(5);
        arr.PushFront(7); // triggers average 6
        Assert.AreEqual(6, arr[1, 0]);
        Assert.AreEqual(2, arr[1, 1]);
    }
}
