using System;
namespace MyConsoleApp.Tests;

[TestClass]
public class QuadraticFitTests
{
    private static void PushValues(CircularMultiResolutionArray<float> arr, int count, Func<int, float> func)
    {
        for (int i = 0; i < count; i++)
        {
            arr.PushFront(func(i));
        }
    }

    [TestMethod]
    public void FitsExactQuadratic()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 16, 2);
        var p = QuadraticFit<float>.CreateParameters(arr, 5, 0);
        var fit = new QuadraticFit<float>(p.sumSrc, p.xySrc, p.x2ySrc, p.to, p.from, p.itemCount);

        PushValues(arr, 6, i => 2f * i * i + 3f * i + 4f);

        Assert.AreEqual(2f, fit.A, 1e-4f);
        Assert.AreEqual(3f, fit.B, 1e-4f);
        Assert.AreEqual(4f, fit.C, 1e-4f);
    }
}
