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
