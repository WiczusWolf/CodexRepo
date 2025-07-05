namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionArrayTests
{
    [TestMethod]
    public void PushFrontAddsToHighResolution()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        arr.PushFront(1f);
        Assert.AreEqual(1f, arr[0, 0]);
        arr.PushFront(2f);
        Assert.AreEqual(2f, arr[0, 0]);
        Assert.AreEqual(1f, arr[0, 1]);
    }

    [TestMethod]
    public void AveragesAreAddedToLowerResolution()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 3, 2);
        arr.PushFront(1f);
        arr.PushFront(3f); // triggers average 2
        Assert.AreEqual(2f, arr[1, 0]);

        arr.PushFront(5f);
        arr.PushFront(7f); // triggers average 6
        Assert.AreEqual(6f, arr[1, 0]);
        Assert.AreEqual(2f, arr[1, 1]);
    }

    [TestMethod]
    public void PushMany()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 3, 2);
        for (int i = 0; i < 100; i++)
        {
            arr.PushFront(i);
        }
    }

    [TestMethod]
    public void HighResolutionCyclesWhenCapacityExceeded()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
        for (int i = 1; i <= 5; i++)
        {
            arr.PushFront(i);
        }
        Assert.AreEqual(3, arr.GetStartIndex(0));
        Assert.AreEqual(5f, arr[0, 0]);
        Assert.AreEqual(2f, arr[0, 3]);
    }

    [TestMethod]
    public void MidResolutionCyclesWhenCapacityExceeded()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
        for (int i = 1; i <= 10; i++)
        {
            arr.PushFront(i);
        }
        Assert.AreEqual(3, arr.GetStartIndex(1));
        Assert.AreEqual(9.5f, arr[1, 0]);
        Assert.AreEqual(3.5f, arr[1, 3]);
    }

    [TestMethod]
    public void LowestResolutionCyclesWhenCapacityExceeded()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
        for (int i = 1; i <= 20; i++)
        {
            arr.PushFront(i);
        }
        Assert.AreEqual(3, arr.GetStartIndex(2));
        Assert.AreEqual(18.5f, arr[2, 0]);
        Assert.AreEqual(6.5f, arr[2, 3]);
    }


    [TestMethod]
    public void OnValueAddedIsInvokedWithAddedValue()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 2, 2);
        float? added = null;
        arr.OnValueAdded[0].Add(i => added = i);
        arr.PushFront(42f);
        Assert.AreEqual(42f, added);
    }

    [TestMethod]
    public void OnValueRemovedIsInvokedWhenItemIsOverwritten()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 2, 2);
        float? removed = null;

        arr.OnValueRemoved[0].Add(i => removed = i);

        arr.PushFront(1f);
        arr.PushFront(2f);
        arr.PushFront(3f); // overwrites 1
        Assert.AreEqual(1f, removed);
    }


    [TestMethod]
    public void OnValueAddedIsInvokedForLowerPartition()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 2, 2);
        float? added = null;
        arr.OnValueAdded[1].Add(i => added = i);
        arr.PushFront(1f);
        arr.PushFront(3f); // triggers average 2
        Assert.AreEqual(2f, added);
    }

    [TestMethod]
    public void NaiveIndexAccessAndIndexStruct()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 3, 2);
        for (int i = 1; i <= 10; i++)
        {
            arr.PushFront(i);
        }

        Assert.AreEqual(10f, arr[1]);
        Assert.AreEqual(9f, arr[2]);
        Assert.AreEqual(8f, arr[3]);
        Assert.AreEqual(7.5f, arr[4]);
        Assert.AreEqual(5.5f, arr[5]);

        var info3 = arr.GetIndex(3);
        Assert.AreEqual(0, info3.Partition);
        Assert.AreEqual(2, info3.PartitionIndex);
        Assert.AreEqual(0, info3.Offset);

        var info4 = arr.GetIndex(4);
        Assert.AreEqual(1, info4.Partition);
        Assert.AreEqual(1, info4.PartitionIndex);
        Assert.AreEqual(1, info4.Offset);

        var info5 = arr.GetIndex(5);
        Assert.AreEqual(1, info5.Partition);
        Assert.AreEqual(2, info5.PartitionIndex);
        Assert.AreEqual(0, info5.Offset);
    }
}
