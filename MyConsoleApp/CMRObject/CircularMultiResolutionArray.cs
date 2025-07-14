using System.Numerics;
using static MyConsoleApp.IntMath;
using static MyConsoleApp.INumberMath;

namespace MyConsoleApp.CMRObject
{
    public class CircularMultiResolutionArray<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        protected readonly T[][] _partitions;
        protected readonly T[] _removed;
        public CircularMultiResolutionArray(int partitionCount, int partitionSize, int magnitudeIncrease)
            : base(partitionCount, partitionSize, magnitudeIncrease, false)
        {
            _removed = new T[_partitionCount];
            _partitions = new T[_partitionCount][];
            for (int i = 0; i < _partitionCount; i++)
            {
                _partitions[i] = new T[_partitionSize];
            }
        }

        public override T First() => GetWithNonCircularItemIndex(_partitions, 0, 0);

        protected override void Assign(int realPartitionIndex, int realItemIndex)
        {
            _removed[realPartitionIndex] = _partitions[realPartitionIndex][realItemIndex];
            _partitions[realPartitionIndex][realItemIndex] = AverageFromPartition(realPartitionIndex - 1);
        }
        protected override void AssignFirst(T value, int realItemIndex)
        {
            _partitions[0][realItemIndex] = value;
        }
        protected override void PostItemPush()
        {
        }
        private T AverageFromPartition(int partitionIndex)
        {
            int realItemIndex = _cursors[partitionIndex] & _partitionSizeMask;
            T sum = T.Zero;
            for (int i = 0; i < _magnitudeIncrease; i++)
            {
                realItemIndex = realItemIndex - 1 & _partitionSizeMask;
                sum += _partitions[partitionIndex][realItemIndex];
            }

            return sum / T.CreateTruncating(_magnitudeIncrease);
        }

        public override T this[CMRIndex index]
        {
            get
            {
                int partitionIndex = index.PartitionIndex;
                int itemIndex = index.ItemIndex;
                int itemOffset = index.Offset;

                T current = GetWithNonCircularItemIndex(_partitions, partitionIndex, itemIndex);
                T next = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1,
                    GetWithNonCircularItemIndex(_partitions, partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)),
                    _removed[partitionIndex]);
                T previous = GetWithNonCircularItemIndex(_partitions, partitionIndex, itemIndex - 1);

                var (offset, maxOffset) = ComputeOffsetFromHalfPartition(partitionIndex, itemOffset);
                return Interpolate(current, previous, next, offset, maxOffset);
            }
        }

    }
}
