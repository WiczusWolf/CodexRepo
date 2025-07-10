namespace MyConsoleApp
{
    public readonly struct IndexInfo
    {
        public IndexInfo(int partitionIndex, int itemIndex, int offset, int naiveIndex, int modulo)
        {
            PartitionIndex = partitionIndex;
            ItemIndex = itemIndex;
            Offset = offset;
            NaiveIndex = naiveIndex;
            Modulo = modulo;
        }

        public int PartitionIndex { get; }
        public int ItemIndex { get; }
        public int Offset { get; }
        public int NaiveIndex { get; }
        public int Modulo { get; }

    }
}
