using MyConsoleApp.CMRObject;

namespace MyConsoleApp
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var arr = new CircularMultiResolutionArray<float>(1, 8, 2);
            var weighted = new CircularMultiResolutionWeightedSum<float>(arr, 1, 8, 2);
            float[] vals = [7.0f, 8.0f, 9.0f];
            for (int i = 0; i < 14; i++)
            {
                if (i == 13)
                {
                }
                int valIndex = i % 3;
                arr.PushFront(vals[valIndex]);
            }
            weighted.ApplyRemoved();
            arr.PushFront(9);
            Console.WriteLine(weighted.ToString());
        }
    }
}
