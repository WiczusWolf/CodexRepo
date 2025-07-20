using MyConsoleApp.CMRObject;

namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionArrayTests
{
    private void AssertCorrectItems(CircularMultiResolutionArray<float> arr, int lastInsert)
    {
        int count = arr.Count;
        for (int i = 0; i < count; i++)
        {
            float acutal = arr[arr.GetIndex(i)];
            float expected = lastInsert - i;
            Assert.AreEqual(acutal, expected);
        }
    }
    [TestMethod]
    public void SimplePush()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 8, 4);

        for (int i = 0; i < 500; i++)
        {
            arr.PushFront(i / 2);
            AssertCorrectItems(arr, i);
        }
    }
}

