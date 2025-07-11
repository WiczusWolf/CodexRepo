namespace MyConsoleApp.Tests;

[TestClass]
public class RunningAverageTests
{
    [TestMethod]
    public void Constructor_Throws_WhenToLessOrEqualFrom()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        Assert.ThrowsException<ArgumentException>(() => new RunningAverage<float>(sum, 0, 0));
        Assert.ThrowsException<ArgumentException>(() => new RunningAverage<float>(sum, 1, 2));
    }

    [TestMethod]
    public void AverageMatchesLastNValues()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        var avg = new RunningAverage<float>(sum, 3, 0);
        Queue<float> last = new();
        float running = 0f;
        for (int i = 1; i <= 5; i++)
        {
            arr.PushFront(i);
            last.Enqueue(i);
            running += i;
            if (last.Count > 3)
            {
                running -= last.Dequeue();
            }
            if (last.Count == 3)
            {
                Assert.AreEqual(running / 3f, avg.LastResult, 1e-5, $"After {i}");
            }
        }
    }

    [TestMethod]
    public void OnItemAdded_RaisedAfterValueAdded()
    {
        var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
        var sum = new CircularMultiResolutionSum<float>(arr, 2, 8, 2);
        var avg = new RunningAverage<float>(sum, 3, 0);
        bool raised = false;
        avg.OnItemAdded.Add(() => raised = true);
        arr.PushFront(1f);
        Assert.IsTrue(raised);
    }
}
