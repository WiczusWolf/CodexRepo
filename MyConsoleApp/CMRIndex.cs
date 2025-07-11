namespace MyConsoleApp
{
    public struct CMRIndex(int partitionIndex, int itemIndex, int offset)
    {
        public readonly int PartitionIndex = partitionIndex;
        public readonly int ItemIndex = itemIndex;
        public readonly int Offset = offset;
    }
}
