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
    public void Constructor_Throws_OnNonPowerOfTwoPartitionSize()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new CircularMultiResolutionArray<float>(2, 3, 2));
    }

    [TestMethod]
    public void Constructor_Throws_OnInvalidMagnitudeIncrease()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new CircularMultiResolutionArray<float>(2, 4, 3));
    }

    [TestMethod]
    public void Constructor_Throws_WhenMagnitudeIncreaseTooLarge()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new CircularMultiResolutionArray<float>(2, 4, 8));
    }

    [TestMethod]
    public void GetIndex_Throws_OnIndexTooLarge()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => arr.GetIndex((uint)(arr.MaxSize + 1)));
    }

    [TestMethod]
    public void CountReflectsNumberOfItems()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        PushSequential(arr, 5);
        Assert.AreEqual(5, arr.Count);
    }

    [TestMethod]
    public void First_ReturnsMostRecentItem()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        arr.PushFront(1);
        arr.PushFront(2);
        Assert.AreEqual(2f, arr.First());
    }

    [TestMethod]
    public void Indexer_ReturnsValuesInReverseOrder()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        PushSequential(arr, 4);
        for (int i = 0; i < arr.Count; i++)
        {
            Assert.AreEqual(3 - i, arr[arr.GetIndex((uint)i)], 1e-6, $"Index {i}");
        }
    }

    [TestMethod]
    public void LinearInterpolation_IsExactForFullArray()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        PushSequential(arr, 8);
        for (int i = 0; i < arr.Count - 1; i++)
        {
            Assert.AreEqual(7 - i, arr[arr.GetIndex((uint)i)], 1e-3, $"Index {i}");
        }
        Assert.IsTrue(Math.Abs(arr[arr.GetIndex((uint)(arr.Count - 1))] - 0f) < 0.5f);
    }

    [TestMethod]
    public void LinearInterpolation_IncompleteArray_AllowsTolerance()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        PushSequential(arr, 5);
        for (int i = 0; i < arr.PartitionSize; i++)
        {
            Assert.AreEqual(4 - i, arr[arr.GetIndex((uint)i)], 1e-5, $"Index {i}");
        }
        Assert.IsTrue(Math.Abs(arr[arr.GetIndex((uint)(arr.Count - 1))] - 0f) < 0.5f);
    }

    [TestMethod]
    public void GetIndex_ProducesCorrectPartitionInfo()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        var idx = arr.GetIndex(5);
        Assert.AreEqual(1, idx.PartitionIndex);
        Assert.AreEqual(2, idx.ItemIndex);
        Assert.AreEqual(1, idx.Offset);
    }

    [TestMethod]
    public void IndexingWrapsAroundCircularly()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        PushSequential(arr, 6);
        Assert.AreEqual(5f, arr[arr.GetIndex(0)]);
        Assert.AreEqual(4f, arr[arr.GetIndex(1)]);
        Assert.AreEqual(3f, arr[arr.GetIndex(2)]);
        Assert.AreEqual(2f, arr[arr.GetIndex(3)]);
    }

    [TestMethod]
    public void EventIsRaisedWhenValueAdded()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        float last = -1f;
        arr.SubscribeValueAdded(() => last = arr.First());
        arr.PushFront(3f);
        Assert.AreEqual(3f, last, 1e-6);
    }
}

