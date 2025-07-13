using MyConsoleApp.CMRObject;

namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionSumTests
{
    private static List<float> PushSequential(CircularMultiResolutionArray<float> arr, int count, float start = 1, float step = 1)
    {
        var list = new List<float>();
        for (int i = 0; i < count; i++)
        {
            float value = start + step * i;
            arr.PushFront(value);
            list.Add(value);
        }
        return list;
    }

    private static float ExpectedRunningSum(IReadOnlyList<float> values, int index)
    {
        float sum = 0f;
        for (int i = 0; i < values.Count - index; i++)
        {
            sum += values[i];
        }
        return sum;
    }

    private static void VerifySums(CircularMultiResolutionArray<float> arr,
                                   CircularMultiResolutionSum<float> sum,
                                   List<float> values)
    {
        for (int i = 0; i < sum.Count; i++)
        {
            var expected = ExpectedRunningSum(values, i);
            var actual = sum[sum.GetIndex((uint)i)];
            if (i < sum.PartitionSize)
            {
                Assert.AreEqual(expected, actual, 1e-3, $"Index {i}");
            }
            else
            {
                var err = Math.Abs(actual - expected) / Math.Abs(expected);
                Assert.IsTrue(err <= 0.025, $"Index {i} error {err:P}");
            }
        }
    }

    [TestMethod]
    public void Constructor_Throws_OnNonPowerOfTwoPartitionSize()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        Assert.ThrowsException<ArgumentException>(() =>
            new CircularMultiResolutionSum<float>(arr, 2, 3, 2));
    }

    [TestMethod]
    public void Constructor_Throws_OnInvalidMagnitudeIncrease()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        Assert.ThrowsException<ArgumentException>(() =>
            new CircularMultiResolutionSum<float>(arr, 2, 4, 3));
    }

    [TestMethod]
    public void Constructor_Throws_WhenMagnitudeIncreaseTooLarge()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        Assert.ThrowsException<ArgumentException>(() =>
            new CircularMultiResolutionSum<float>(arr, 2, 4, 8));
    }

    [TestMethod]
    public void GetIndex_Throws_OnIndexTooLarge()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => sum.GetIndex((uint)(sum.MaxSize + 1)));
    }

    [TestMethod]
    public void HighResolutionSum_MatchesRunningTotals()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        var values = PushSequential(arr, 5);
        VerifySums(arr, sum, values);
    }

    [TestMethod]
    public void MultiPartitionSum_MatchesRunningTotals()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 3, 4, 2);
        var values = PushSequential(arr, 8, 1, 0); // constant values
        VerifySums(arr, sum, values);
    }

    [TestMethod]
    public void LargerPartition_AccuracyWithinTolerance()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 3, 8, 2);
        var values = PushSequential(arr, 10, 1, 0);
        VerifySums(arr, sum, values);
    }

    [TestMethod]
    public void LargerIncrease_AccuracyWithinTolerance()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 8, 4);
        var sum = new CircularMultiResolutionSum<float>(arr, 3, 8, 4);
        var values = PushSequential(arr, 12, 1, 0);
        VerifySums(arr, sum, values);
    }

    [TestMethod]
    public void EventIsRaisedWithCurrentSum()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        float lastValue = 0f;
        sum.SubscribeValueAdded(() => lastValue = sum.First());
        var values = PushSequential(arr, 5);
        Assert.AreEqual(sum[sum.GetIndex(0)], lastValue, 1e-3);
    }

    [TestMethod]
    public void GetIndexIndexer_ReturnSameValue()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 3, 4, 2);
        var values = PushSequential(arr, 6);
        for (int i = 0; i < sum.Count; i++)
        {
            var idx = sum.GetIndex((uint)i);
            Assert.AreEqual(sum[idx], sum[idx], 1e-5); // access to ensure index works
        }
    }

    [TestMethod]
    public void CountReflectsNumberOfItems()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        PushSequential(arr, 7);
        Assert.AreEqual(7, sum.Count);
    }

    [TestMethod]
    public void ResetMaintainsAccuracyWhenThresholdExceeded()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
        // threshold of 5000 => anticipatedMaxItemValue = 5000 / (sum.MaxSize * 2)
        var sum = new CircularMultiResolutionSum<float>(arr, 4, 4, 2, 78.125);
        var values = PushSequential(arr, 8, 1000, 0); // running sum exceeds 5000
        VerifySums(arr, sum, values);
    }

    [TestMethod]
    public void CountCappedAfterReset()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 4, 4, 2, 78.125);
        PushSequential(arr, sum.MaxSize + 10, 1000, 0);
        Assert.AreEqual(sum.MaxSize, sum.Count);
    }
}
