using System.Numerics;

namespace MyConsoleApp
{
    public partial class CircularMultiResolutionArray<T> where T : INumber<T>
    {
        public readonly struct IndexInfo
        {
            public IndexInfo(int partition, int partitionIndex, int offset)
            {
                Partition = partition;
                PartitionIndex = partitionIndex;
                Offset = offset;
            }

            public int Partition { get; }
            public int PartitionIndex { get; }
            public int Offset { get; }
        }
    }
}
