using MyConsoleApp.CMRObject;

namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionArrayTests
{
    private static void PushSequential(CircularMultiResolutionArray<float> arr, int count)
    {
        for (int i = 0; i < count; i++)
        {
            arr.PushFront(i);
        }
    }


    [TestMethod]
    public void SimplePush()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);

        for (int i = 0; i < 4; i++)
        {
            arr.PushFront(i);
            Assert.AreEqual(arr.Count, i + 1);
            for (int j = 0; j < i; j++)
            {
                Assert.AreEqual(i - j, arr[arr.GetIndex(j)]);
            }
        }
    }

    [TestMethod]
    public void PushPastMax()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);

        for (int i = 0; i < 5; i++)
        {
            arr.PushFront(i);
        }
        Assert.AreEqual(arr.Count, 4);
        for (int i = 0; i < 4; i++)
        {
            Assert.AreEqual(4 - i, arr[arr.GetIndex(i)]);
        }
    }

    [TestMethod]
    public void PushIntoSecondPartition()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);

        for (int i = 0; i < 15; i++)
        {
            arr.PushFront(i);
            for (int j = 0; j <= Math.Min(i, 7); j++)
            {
                Assert.AreEqual(i - j, arr[arr.GetIndex(j)]);
            }
        }
    }

    [TestMethod]
    public void PushIntoBigArray()
    {
        var arr = new CircularMultiResolutionArray<float>(4, 16, 4);
        Assert.AreEqual(4 * 4 * 4 * 16, arr.MaxSize);

        int i = 0;
        for (; i < arr.MaxSize; i++)
        {
            arr.PushFront(i);
        }
        for (; i < arr.MaxSize * 2; i++)
        {
            arr.PushFront(i);
            for (int j = 0; j <= arr.MaxSize - 1; j++)
            {
                Assert.AreEqual(i - j, arr[arr.GetIndex(j)]);
            }
        }
    }
}

