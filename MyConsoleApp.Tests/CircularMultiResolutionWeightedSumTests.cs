namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionWeightedSumTests
{
    private static float NaiveWeightedSum(IReadOnlyList<float> values, int maxSize)
    {
        int count = Math.Min(values.Count, maxSize);
        int start = values.Count - count;
        float sum = 0f;
        for (int i = 0; i < count; i++)
        {
            sum += i * values[start + i];
        }
        return sum;
    }

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

    private static float ExpectedWeightedSum(IReadOnlyList<float> values, int index)
    {
        float sum = 0f;
        for (int i = 0; i < values.Count - index; i++)
        {
            sum += i * values[index + i];
        }
        return sum;
    }

    private static void VerifyValuesFinite(CircularMultiResolutionWeightedSum<float> ws)
    {
        for (int i = 0; i < ws.Count; i++)
        {
            var value = ws[ws.GetIndex((uint)i)];
            Assert.IsFalse(float.IsNaN(value) || float.IsInfinity(value), $"Index {i}");
        }
    }
    private float OscillatingValue(int i)
    {
        int mod2 = i % 2;
        int sign = (1 - 2 * ((i / 2) % 2));
        return mod2 * sign;
    }
    [TestMethod]
    public void WeightedSum_MatchesManualCalculation()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var ws = new CircularMultiResolutionWeightedSum<float>(arr, 1, 8, 2);
        List<float> inserted = new();
        foreach (var v in Enumerable.Range(0, 10).Select(OscillatingValue))
        {
            arr.PushFront(v);
            inserted.Add(v);
            Assert.AreEqual(NaiveWeightedSum(inserted, ws.PartitionSize), ws.First(), 1e-4);
        }
    }

    [TestMethod]
    public void ApplyRemoved_MaintainsAccuracy()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var ws = new CircularMultiResolutionWeightedSum<float>(arr, 1, 8, 2);

        List<float> all = new();
        foreach (var v in Enumerable.Range(0, 12).Select(OscillatingValue))
        {
            arr.PushFront(v);
            all.Add(v);
            Assert.AreEqual(NaiveWeightedSum(all, ws.PartitionSize), ws.First(), 1e-4);
        }

        ws.ApplyRemoved();

        foreach (var v in Enumerable.Range(12, 3).Select(OscillatingValue))
        {
            arr.PushFront(v);
            all.Add(v);
            Assert.AreEqual(NaiveWeightedSum(all, ws.PartitionSize), ws.First(), 1e-4);
        }
    }

    [TestMethod]
    public void ApplyRemoved_BeforeWindowFilled_DoesNotThrow()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var ws = new CircularMultiResolutionWeightedSum<float>(arr, 1, 8, 2);

        arr.PushFront(1f);
        arr.PushFront(-1f);

        ws.ApplyRemoved();

        arr.PushFront(1f);

        // Ensure no exception and value is finite
        float val = ws.First();
        Assert.IsFalse(float.IsNaN(val) || float.IsInfinity(val));
    }

    [TestMethod]
    public void MultiPartitionWeightedSum_MatchesRunningTotals() //nonsensical test
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        var ws = new CircularMultiResolutionWeightedSum<float>(arr, 3, 4, 2);
        var values = PushSequential(arr, 8, 1, 0); // constant values
        Assert.AreEqual(ExpectedWeightedSum(values, 0), ws.First(), 1e-3);
        VerifyValuesFinite(ws);
    }

    private void AssertArraysEqual(CircularMultiResolutionWeightedSum<float> flatArray, CircularMultiResolutionWeightedSum<float> multiPartitionArray, int offset = 0)
    {
        Assert.AreEqual(flatArray.MaxSize, multiPartitionArray.MaxSize);
        Assert.AreEqual(flatArray.PartitionSize, multiPartitionArray.PartitionSize);
        Assert.AreEqual(flatArray.MagnitudeIncrease, multiPartitionArray.MagnitudeIncrease);
        int magnitudeIncrease = flatArray.MagnitudeIncrease;

        for (int i = 0; i < flatArray.MaxSize; i++)
        {
            CMRIndex index = flatArray.GetIndex((uint)i);
            float expected = flatArray[index];
            float actual = multiPartitionArray[index];

            if (index.PartitionIndex == 0)
            {
                Assert.AreEqual(expected, actual);
            }
            if (index.PartitionIndex >= 0)
            {
                if ((flatArray.PartitionSize + index.ItemIndex - offset) % flatArray.PartitionSize == 0)
                {
                    Assert.AreEqual(expected, actual);
                }
                else
                {
                    float allowedError = (1 - 1 / (float)magnitudeIncrease) * expected;
                    Assert.AreEqual(expected, actual, allowedError);
                }
            }

        }

    }

    [TestMethod]
    public void MultiPartitionWeightedSum_IncreasesCorrectly()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 4, 2);
        var ws = new CircularMultiResolutionWeightedSum<float>(arr, 3, 4, 2);
        var wsSinglePartition = new CircularMultiResolutionWeightedSum<float>(arr, 1, 32, 2);
        int i = 0;
        for (; i < 32; i++)
        {
            arr.PushFront(1 + OscillatingValue(i));
        }
        AssertArraysEqual(ws, wsSinglePartition);
    }
}
