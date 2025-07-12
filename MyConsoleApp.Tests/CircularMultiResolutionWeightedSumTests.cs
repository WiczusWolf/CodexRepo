namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionWeightedSumTests
{
    [TestMethod]
    public void HarmonicSequence_WeightedSumEqualsItemCount()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var weighted = new CircularMultiResolutionWeightedSum<float>(arr, 2, 8, 2);

        for (int n = 1; n <= 5; n++)
        {
            arr.PushFront(1f / n);
            Assert.AreEqual(n, weighted[weighted.GetIndex(0)], 1e-4, $"After {n} items");
        }
    }

    [TestMethod]
    public void CountCappedAfterReset()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var weighted = new CircularMultiResolutionWeightedSum<float>(arr, 2, 8, 2);
        for (int i = 0; i < weighted.MaxSize + 5; i++)
        {
            arr.PushFront(1f);
        }

        Assert.AreEqual(weighted.MaxSize, weighted.Count);
    }
    [TestMethod]
    public void RepeatingData()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var weighted = new CircularMultiResolutionWeightedSum<float>(arr, 1, 8, 2);

        arr.PushFront(7);
        Assert.AreEqual(weighted[weighted.GetIndex(0)], 0, 1e-4, $"Count:1,Index:0");
        arr.PushFront(8);
        Assert.AreEqual(weighted[weighted.GetIndex(0)], 8, 1e-4, $"Count:1,Index:0");
        Assert.AreEqual(weighted[weighted.GetIndex(1)], 0, 1e-4, $"Count:1,Index:0");
        arr.PushFront(9);
        Assert.AreEqual(weighted[weighted.GetIndex(0)], 26, 1e-4, $"Count:1,Index:0");
        Assert.AreEqual(weighted[weighted.GetIndex(1)], 8, 1e-4, $"Count:1,Index:0");
        Assert.AreEqual(weighted[weighted.GetIndex(2)], 0, 1e-4, $"Count:1,Index:0");
        arr.PushFront(7);//4
        arr.PushFront(8);//5
        arr.PushFront(9);//6
        arr.PushFront(7);//7
        arr.PushFront(8);//8 items
        arr.PushFront(9);
        var a = weighted[weighted.GetIndex(0)];
        Assert.AreEqual(weighted[weighted.GetIndex(0)], 229, 1e-4, $"Count:1,Index:0");

    }

    [TestMethod]
    public void WeightedSumHandlesRemovalGracefully()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var weighted = new CircularMultiResolutionWeightedSum<float>(arr, 2, 8, 2);

        for (int i = 0; i < weighted.MaxSize + 4; i++)
        {
            arr.PushFront(1f);
        }

        Assert.AreEqual(weighted.MaxSize, weighted.Count);
        Assert.IsTrue(weighted[weighted.GetIndex(0)] > 0f);
    }
}
