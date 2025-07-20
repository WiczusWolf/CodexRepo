using System.Numerics;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp.CMRObject
{
    public class CircularMultiResolutionArray<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        protected readonly T[][] _averages;
        protected readonly T[][] _biases;
        protected readonly T[] _removedAverages;
        protected readonly T[] _removedBiases;
        public CircularMultiResolutionArray(int partitionCount, int partitionSize, int magnitudeIncrease)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
            _removedBiases = new T[_partitionCount];
            _removedAverages = new T[_partitionCount];
            _averages = new T[_partitionCount][];
            _biases = new T[_partitionCount][];
            for (int i = 0; i < _partitionCount; i++)
            {
                _averages[i] = new T[_partitionSize];
                _biases[i] = new T[_partitionSize];
            }
        }

        public override T First() => GetWithNonCircularItemIndex(_averages, 0, 0);

        protected override void Assign(int realPartitionIndex, int realItemIndex)
        {
            _removedAverages[realPartitionIndex] = _averages[realPartitionIndex][realItemIndex];
            _removedBiases[realPartitionIndex] = _biases[realPartitionIndex][realItemIndex];
            _averages[realPartitionIndex][realItemIndex] = AverageFromPartition(realPartitionIndex - 1);
            _biases[realPartitionIndex][realItemIndex] = _biases[realPartitionIndex - 1][_magnitudeIncrease - 1];
            _biases[realPartitionIndex][realItemIndex] = GetWithNonCircularItemIndex(_biases, 0, 0);
        }
        protected override void AssignFirst(T value, int realItemIndex)
        {
            _averages[0][realItemIndex] = value;
            _biases[0][realItemIndex] = value;
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
                sum += _averages[partitionIndex][realItemIndex];
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


                CMRIndex currentIndex = CurrentIndex(index);
                T currentBias = GetWithNonCircularItemIndex(_biases, currentIndex.PartitionIndex, currentIndex.ItemIndex);
                T currentAverage = GetWithNonCircularItemIndex(_averages, currentIndex.PartitionIndex, currentIndex.ItemIndex);
                int itemCount = currentIndex.Modulo;
                T delta = T.CreateTruncating(2) * (currentBias - currentAverage) / T.CreateTruncating(FastMax(itemCount - 1, 1));
                return currentBias - delta * T.CreateTruncating(currentIndex.Offset);
            }
        }

    }
}
