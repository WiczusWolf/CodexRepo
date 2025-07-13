//using MyConsoleApp.CMRObject;
//using MyConsoleApp.ReductionObjects;

//namespace MyConsoleApp.Tests;

//[TestClass]
//public class StandardDeviationTests
//{
//    [TestMethod]
//    public void Constructor_Throws_WhenSourcesHaveDifferentSize()
//    {
//        var arr1 = new CircularMultiResolutionArray<float>(1, 8, 2);
//        var arr2 = new CircularMultiResolutionArray<float>(1, 4, 2);

//        var square = new SquaredValue<float>(arr1, arr1.GetIndex(0));
//        var squareSum = new CircularMultiResolutionSum<float>(square, arr1.PartitionCount, arr1.PartitionSize, arr1.MagnitudeIncrease);
//        var sum = new CircularMultiResolutionSum<float>(arr2, arr2.PartitionCount, arr2.PartitionSize, arr2.MagnitudeIncrease);

//        Assert.ThrowsException<ArgumentException>(() =>
//            new StandardDeviation<float>(squareSum, sum, arr1.GetIndex(1), arr1.GetIndex(0), 2));
//    }

//    [TestMethod]
//    public void Constructor_Throws_WhenItemCountTooSmall()
//    {
//        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
//        var square = new SquaredValue<float>(arr, arr.GetIndex(0));
//        var squareSum = new CircularMultiResolutionSum<float>(square, 2, 8, 2);
//        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);

//        Assert.ThrowsException<ArgumentException>(() =>
//            new StandardDeviation<float>(squareSum, sum, arr.GetIndex(1), arr.GetIndex(0), 1));
//    }

//    [TestMethod]
//    public void ConstantSequence_YieldsZero()
//    {
//        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
//        var p = StandardDeviation<float>.CreateParameters(arr, 3, 0);
//        var sd = new StandardDeviation<float>(p.squaredSumSrc, p.sumSrc, p.to, p.from, p.itemCount);

//        for (int i = 0; i < 4; i++)
//        {
//            arr.PushFront(1f);
//        }

//        Assert.AreEqual(0f, sd.Value, 1e-5);
//    }

//    [TestMethod]
//    public void MatchesManualCalculation()
//    {
//        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
//        var p = StandardDeviation<float>.CreateParameters(arr, 3, 0);
//        var sd = new StandardDeviation<float>(p.squaredSumSrc, p.sumSrc, p.to, p.from, p.itemCount);

//        for (int i = 1; i <= 4; i++)
//        {
//            arr.PushFront(i);
//        }

//        float[] values = { 2f, 3f, 4f };
//        float mean = (values[0] + values[1] + values[2]) / 3f;
//        float variance = ((values[0] - mean) * (values[0] - mean) +
//                          (values[1] - mean) * (values[1] - mean) +
//                          (values[2] - mean) * (values[2] - mean)) / (values.Length - 1);
//        float expected = (float)Math.Sqrt(variance);

//        Assert.AreEqual(expected, sd.Value, 1e-5);
//    }
//}
