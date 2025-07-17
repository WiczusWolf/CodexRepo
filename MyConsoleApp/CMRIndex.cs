using System.Diagnostics.CodeAnalysis;
using System.Text;
using static MyConsoleApp.IntMath;
namespace MyConsoleApp
{
    public readonly struct CMRIndex(int partitionIndex, int itemIndex, int offset, int modulo)
    {
        public readonly int PartitionIndex = partitionIndex;
        public readonly int ItemIndex = itemIndex;
        public readonly int Offset = offset;
        public readonly int Modulo = modulo;

        public CMRIndex AddOffset(int offset)
        {
            int tempOffset = Offset + offset;
            int tempItemIndex = ItemIndex + DivZeroForRange(tempOffset, 0, Modulo);
            return new CMRIndex(PartitionIndex, tempItemIndex, FastAbsMod(tempOffset, Modulo), Modulo);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            bool result = true;
            if (obj is CMRIndex other)
            {
                result &= PartitionIndex == other.PartitionIndex;
                result &= ItemIndex == other.ItemIndex;
                result &= Offset == other.Offset;
                result &= Modulo == other.Modulo;
                return result;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Partition Index: {PartitionIndex} ");
            sb.Append($"Item Index: {ItemIndex} ");
            sb.Append($"Offset: {Offset} ");
            sb.Append($"Modulo: {Modulo}. ");
            return sb.ToString();
        }
    }
}
