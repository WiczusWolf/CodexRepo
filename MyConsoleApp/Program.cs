namespace MyConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CircularMultiResolutionArray<double> src = new CircularMultiResolutionArray<double>(3, 8, 4);
            var parameters = StandardDeviation<double>.CreateParameters(src, 10);
            StandardDeviation<double> standardDeviation = new(parameters.squaredSumSrc, parameters.sumSrc, parameters.to, parameters.from, parameters.itemCount);

            double entry = 10;
            for (int i = 0; i < 15; i++)
            {
                src.PushFront(entry);
                Console.WriteLine(standardDeviation.Value);
            }

            for (int i = 0; i < 15; i++)
            {
                src.PushFront(entry + Random.Shared.Next(10) / 5 - 1);
                Console.WriteLine(standardDeviation.Value);
            }

        }
    }
}
