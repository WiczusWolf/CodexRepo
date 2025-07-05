using System.Numerics;

namespace MyConsoleApp
{
    public partial class CircularMultiResolutionArray<T> where T : INumber<T>
    {
        public readonly struct IndexInfo
        {
            public IndexInfo(int partitionIndex, int itemIndex, int offset)
            {
                PartitionIndex = partitionIndex;
                ItemIndex = itemIndex;
                Offset = offset;
            }

            public int PartitionIndex { get; }
            public int ItemIndex { get; }
            public int Offset { get; }
        }
    }
}
