namespace MyConsoleApp.Tests;

[TestClass]
public class IntMathTests
{
    [TestMethod]
    public void IsPowerOfTwoDetectsPowersOfTwo()
    {
        Assert.IsTrue(IntMath.IsPowerOfTwo(1));
        Assert.IsTrue(IntMath.IsPowerOfTwo(2));
        Assert.IsTrue(IntMath.IsPowerOfTwo(4));
        Assert.IsFalse(IntMath.IsPowerOfTwo(0));
        Assert.IsFalse(IntMath.IsPowerOfTwo(3));
        Assert.IsFalse(IntMath.IsPowerOfTwo(-8));
    }

    [TestMethod]
    public void FastMaxReturnsLargerValue()
    {
        Assert.AreEqual(10, IntMath.FastMax(10, 5));
        Assert.AreEqual(10, IntMath.FastMax(5, 10));
        Assert.AreEqual(7, IntMath.FastMax(7, 7));
    }

    [TestMethod]
    public void FastMinReturnsSmallerValue()
    {
        Assert.AreEqual(5, IntMath.FastMin(10, 5));
        Assert.AreEqual(5, IntMath.FastMin(5, 10));
        Assert.AreEqual(7, IntMath.FastMin(7, 7));
    }

    [TestMethod]
    public void FastAbsReturnsAbsoluteValue()
    {
        Assert.AreEqual(5, IntMath.FastAbs(5));
        Assert.AreEqual(5, IntMath.FastAbs(-5));
        Assert.AreEqual(0, IntMath.FastAbs(0));
    }

    [TestMethod]
    public void PowComputesExponentiation()
    {
        Assert.AreEqual(1, IntMath.Pow(2, 0));
        Assert.AreEqual(2, IntMath.Pow(2, 1));
        Assert.AreEqual(8, IntMath.Pow(2, 3));
        Assert.AreEqual(27, IntMath.Pow(3, 3));
    }
}
