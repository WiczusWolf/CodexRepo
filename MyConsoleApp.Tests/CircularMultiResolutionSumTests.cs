namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionSumTests
{
    [TestMethod]
    public void FirstResolution()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr);

        arr.PushFront(1f);
        arr.PushFront(2f);
        arr.PushFront(3f);
        arr.PushFront(4f);

        Assert.AreEqual(10f, sum[0]); // 10-0
        Assert.AreEqual(6f, sum[1]);  // 6 -0
        Assert.AreEqual(3f, sum[2]);  // 3 -0
        Assert.AreEqual(1f, sum[3]);  // 1 -0
    }

    [TestMethod]
    public void FirstResolutionOverflowTest1()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr);

        arr.PushFront(1f);
        arr.PushFront(2f);
        arr.PushFront(3f);
        arr.PushFront(4f);

        arr.PushFront(5f); // Pushes 5 and removes 1. 

        Assert.AreEqual(14f, sum[0]); // 15-1
        Assert.AreEqual(9f, sum[1]);  // 10-1
        Assert.AreEqual(5f, sum[2]);  // 6 -1
        Assert.AreEqual(2f, sum[3]);  // 3 -1
    }

    [TestMethod]
    public void FirstResolutionOverflowTest2()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr);

        arr.PushFront(1f);
        arr.PushFront(2f);
        arr.PushFront(3f);
        arr.PushFront(4f);

        arr.PushFront(5f); // Pushes 5 and removes 1. 
        arr.PushFront(6f); // Pushes 5 and removes 2. 

        Assert.AreEqual(18f, sum[0]); // 21-3
        Assert.AreEqual(12f, sum[1]); // 15-3
        Assert.AreEqual(7f, sum[2]);  // 10-3
        Assert.AreEqual(3f, sum[3]);  // 6 -3
    }

    [TestMethod]
    public void SecondResolution()
    {
        var arr = new CircularMultiResolutionArray<float>(2, 3, 2);
        var sum = new CircularMultiResolutionSum<float>(arr);

        arr.PushFront(1f);
        //Pushes 1 to running sum first resolution.
        arr.PushFront(2f); //Pushes 1.5 to second resolution. 
        //Pushes 3 to running sum first resolution. Pushes 3 to running sum second resolution. The sum correction is now 0.
        arr.PushFront(3f);
        //Pushes 6 to running sum first resolution.
        arr.PushFront(4f); //Pushes 3.5 to second resolution.
        //Pushes 10 to running sum first resolution. Pushes 10 to running sum second resolution. The sum correction is now 0.
        arr.PushFront(5f);
        //Pushes 15 to running sum first resolution. 
        arr.PushFront(6f); //Pushes 5.5 to second resolution. 
        //Pushes 21 to running sum first resolution. Pushes 21 to running sum second resolution. The sum correction is now 0.
        arr.PushFront(7f);
        //Pushes 28 to running sum first resolution.
        arr.PushFront(8f); //Pushes 7.5 to second resolution and removes 1.5 item. 
        //Pushes 36 to running sum first resolution. Pushes 36 to running sum second resolution and removes 3. The sum correction is now 3.
        arr.PushFront(9f);
        //Pushes 45 to running sum first resolution.
        arr.PushFront(10f); //Pushes 9.5 to second resolution and removes 3.5 item. 
        //Pushes 55 to running sum first resolution. Pushes 55 to running sum second resolution and removes 10. The sum correction is now 10.

        //When retrieving subtract correction number (10).
        //Since the second resolution was just pushed into the result is expected to be accurate.
        //The running sum at index 0 is 55. 
        //The mathematically correct result is 10+9+8+7+6+5=45.
        Assert.AreEqual(45f, sum[0]);// partition 0 index 0
        //The running sum at index 1 is 45. 
        //The mathematically correct result is 9+8+7+6+5=35.
        Assert.AreEqual(35f, sum[1]);// partition 0 index 1
        //The running sum at index 2 is 36. 
        //The mathematically correct result is 8+7+6+5=26.
        Assert.AreEqual(26f, sum[2]);// partition 0 index 2

        //The running sum at index 3 is 36.
        //We know that between 36 and 21, 15 was added in total.
        //So on average 7.5 was added.
        //Since we want the element in the middle 36 - 7.5 - 10  = 18.5. 
        //The mathematically correct result is 7+6+5=18.
        //The resolution decreased and we use a fraction this means some information was lost and this small error is acceptable. 
        Assert.AreEqual(18.5f, sum[3]);// partition 1 index 1

        //The running sum at index 4 (second partition first item) is 21.
        //The mathematically correct result is 6+5=11.
        //We don't use the fraction of the resolution this means there should be no error. 
        Assert.AreEqual(11f, sum[4]);// partition 1 index 1.5

        //The running sum at index 5  is 21.
        //We know that between 21 and the correction variable 10, 11 was added in total.
        //So on average 5.5 was added.
        //Since we want the element in the middle 21 - 5.5 - 10  = 5.5. 
        //The mathematically correct result is 5.
        //The resolution decreased and we use a fraction this means some information was lost and this small error is acceptable. 
        Assert.AreEqual(5.5f, sum[5]);// partition 1 index 2
    }

    [TestMethod]
    public void IndexInfoIndexerReturnsCorrectValue()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
        var sum = new CircularMultiResolutionSum<float>(arr);

        arr.PushFront(1f);
        arr.PushFront(2f);
        arr.PushFront(3f);
        arr.PushFront(4f);

        for (int i = 0; i < 4; i++)
        {
            var info = arr.GetIndex(i);
            Assert.AreEqual(sum[i], sum[info]);
        }
    }
}
