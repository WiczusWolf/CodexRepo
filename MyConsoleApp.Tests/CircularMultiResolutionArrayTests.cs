namespace MyConsoleApp.Tests;

[TestClass]
public class CircularMultiResolutionArrayTests
{
	[TestMethod]
	public void PushFrontAddsToHighResolution()
	{
		var arr = new CircularMultiResolutionArray<float>(1, 4, 2);
		arr.PushFront(1f);
		Assert.AreEqual(1f, arr[0, 0]);
		arr.PushFront(2f);
		Assert.AreEqual(2f, arr[0, 0]);
		Assert.AreEqual(1f, arr[0, 1]);
	}

	[TestMethod]
	public void AveragesAreAddedToLowerResolution()
	{
		var arr = new CircularMultiResolutionArray<float>(2, 3, 2);
		arr.PushFront(1f);
		arr.PushFront(3f); // triggers average 2
		Assert.AreEqual(2f, arr[1, 0]);

		arr.PushFront(5f);
		arr.PushFront(7f); // triggers average 6
		Assert.AreEqual(6f, arr[1, 0]);
		Assert.AreEqual(2f, arr[1, 1]);
	}

	[TestMethod]
	public void PushMany()
	{
		var arr = new CircularMultiResolutionArray<float>(2, 3, 2);
		for (int i = 0; i < 100; i++)
		{
			arr.PushFront(i);
		}
	}

	[TestMethod]
	public void HighResolutionCyclesWhenCapacityExceeded()
	{
		var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
		for (int i = 1; i <= 5; i++)
		{
			arr.PushFront(i);
		}
		Assert.AreEqual(3, arr.GetStartIndex(0));
		Assert.AreEqual(5f, arr[0, 0]);
		Assert.AreEqual(2f, arr[0, 3]);
	}

	[TestMethod]
	public void MidResolutionCyclesWhenCapacityExceeded()
	{
		var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
		for (int i = 1; i <= 10; i++)
		{
			arr.PushFront(i);
		}
		Assert.AreEqual(3, arr.GetStartIndex(1));
		Assert.AreEqual(9.5f, arr[1, 0]);
		Assert.AreEqual(3.5f, arr[1, 3]);
	}

	[TestMethod]
	public void LowestResolutionCyclesWhenCapacityExceeded()
	{
		var arr = new CircularMultiResolutionArray<float>(3, 4, 2);
		for (int i = 1; i <= 20; i++)
		{
			arr.PushFront(i);
		}
		Assert.AreEqual(3, arr.GetStartIndex(2));
		Assert.AreEqual(18.5f, arr[2, 0]);
		Assert.AreEqual(6.5f, arr[2, 3]);
	}


	[TestMethod]
	public void OnItemAddedIsInvokedWithAddedValue()
	{
		var arr = new CircularMultiResolutionArray<float>(1, 2, 2);
		float? added = null;
		arr.OnItemAdded.Add(i => added = i);
		arr.PushFront(42f);
		Assert.AreEqual(42f, added);
	}

	[TestMethod]
	public void OnItemRemovedIsInvokedWhenItemIsOverwritten()
	{
		var arr = new CircularMultiResolutionArray<float>(1, 2, 2);
		float? removed = null;

		arr.OnItemRemoved.Add(i => removed = i);

		arr.PushFront(1f);
		arr.PushFront(2f);
		arr.PushFront(3f); // overwrites 1
		Assert.AreEqual(1f, removed);
	}

}
