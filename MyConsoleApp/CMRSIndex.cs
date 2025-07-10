namespace MyConsoleApp
{
    public struct CMRSIndex(int partitionIndex, int itemIndex, int offset)
    {
        public readonly int PartitionIndex = partitionIndex;
        public readonly int ItemIndex = itemIndex;
        public readonly int Offset = offset;
    }
}
