namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionArrayTests
{
	[TestMethod]
	public void PushFrontAddsToHighResolution()
	{
		var arr = new CircularMultiResolutionArray<int>(1, 4, 2);
		arr.PushFront(1);
		Assert.AreEqual(1, arr[0, 0]);
		arr.PushFront(2);
		Assert.AreEqual(2, arr[0, 0]);
		Assert.AreEqual(1, arr[0, 1]);
	}

	[TestMethod]
	public void AveragesAreAddedToLowerResolution()
	{
		var arr = new CircularMultiResolutionArray<int>(2, 3, 2);
		arr.PushFront(1);
		arr.PushFront(3); // triggers average 2
		Assert.AreEqual(2, arr[1, 0]);

		arr.PushFront(5);
		arr.PushFront(7); // triggers average 6
		Assert.AreEqual(6, arr[1, 0]);
		Assert.AreEqual(2, arr[1, 1]);
	}

        [TestMethod]
        public void PushMany()
        {
                var arr = new CircularMultiResolutionArray<int>(2, 3, 2);
                for (int i = 0; i < 100; i++)
                {
                        arr.PushFront(i);
                }
        }

        [TestMethod]
        public void HighResolutionCyclesWhenCapacityExceeded()
        {
                var arr = new CircularMultiResolutionArray<int>(3, 4, 2);
                for (int i = 1; i <= 5; i++)
                {
                        arr.PushFront(i);
                }
                Assert.AreEqual(3, arr.GetStartIndex(0));
                Assert.AreEqual(5, arr[0, 0]);
                Assert.AreEqual(2, arr[0, 3]);
        }

        [TestMethod]
        public void MidResolutionCyclesWhenCapacityExceeded()
        {
                var arr = new CircularMultiResolutionArray<int>(3, 4, 2);
                for (int i = 1; i <= 10; i++)
                {
                        arr.PushFront(i);
                }
                Assert.AreEqual(3, arr.GetStartIndex(1));
                Assert.AreEqual(9, arr[1, 0]);
                Assert.AreEqual(3, arr[1, 3]);
        }

        [TestMethod]
        public void LowestResolutionCyclesWhenCapacityExceeded()
        {
                var arr = new CircularMultiResolutionArray<int>(3, 4, 2);
                for (int i = 1; i <= 20; i++)
                {
                        arr.PushFront(i);
                }
                Assert.AreEqual(3, arr.GetStartIndex(2));
                Assert.AreEqual(18, arr[2, 0]);
                Assert.AreEqual(6, arr[2, 3]);
        }

}
