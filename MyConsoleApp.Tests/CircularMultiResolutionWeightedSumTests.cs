using System;
using System.Collections.Generic;
using System.Linq;

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

    [TestMethod]
    public void WeightedSum_MatchesManualCalculation()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var ws = new CircularMultiResolutionWeightedSum<float>(arr, 1, 8, 2);
        List<float> inserted = new();
        foreach (var v in Enumerable.Range(0, 10).Select(i => MathF.Sin(i * MathF.PI / 2f)))
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
        foreach (var v in Enumerable.Range(0, 12).Select(i => MathF.Sin(i * MathF.PI / 2f)))
        {
            arr.PushFront(v);
            all.Add(v);
            Assert.AreEqual(NaiveWeightedSum(all, ws.PartitionSize), ws.First(), 1e-4);
        }

        ws.ApplyRemoved();

        foreach (var v in Enumerable.Range(12, 3).Select(i => MathF.Sin(i * MathF.PI / 2f)))
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
}
