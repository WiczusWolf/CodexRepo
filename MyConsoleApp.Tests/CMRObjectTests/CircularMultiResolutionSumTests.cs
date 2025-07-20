using MyConsoleApp.CMRObject;

namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionSumTests
{
    private static int Prediction(int i, int divisor)
    {
        int quotient = i / divisor;
        int naiveSum = ((quotient + 1) * quotient * divisor) / 2;
        int offset = quotient * (divisor - i % divisor - 1);
        return naiveSum - offset;
    }
    private void AssertCorrectItems(CircularMultiResolutionSums<float> sums, int i, int divisor)
    {
        int count = sums.Count;
        float predAdjustment = 0;

        if (i > sums.MaxSize)
        {
            predAdjustment -= Prediction(i - sums.MaxSize, divisor);
        }

        for (int j = 0; j <= Math.Min(i, sums.MaxSize - 1); j++)
        {
            float acutal = sums[sums.GetIndex(j)];
            float expected = Prediction(i - j, divisor) + predAdjustment;
            Assert.AreEqual(expected, acutal);
        }
    }
    [TestMethod]
    public void SimplePush()
    {
        var arr = new CircularMultiResolutionArray<float>(3, 8, 4);
        int magnitudeStep = 2;
        var sums = new CircularMultiResolutionSums<float>(arr, 2, magnitudeStep * 2, magnitudeStep);

        for (int i = magnitudeStep; i < 35; i++)
        {
            arr.PushFront(i / magnitudeStep);
            AssertCorrectItems(sums, i, magnitudeStep);
        }
    }
    [TestMethod]
    public void BigPush()
    {
        int magnitudeStep = 4;
        var arr = new CircularMultiResolutionArray<float>(3, 8, 4);
        var sums = new CircularMultiResolutionSums<float>(arr, 3, magnitudeStep * 2, magnitudeStep);
        int divisor = magnitudeStep * magnitudeStep;
        for (int i = divisor; i < 50; i++)
        {
            arr.PushFront(i / divisor);
            AssertCorrectItems(sums, i, divisor);
        }
    }
}
