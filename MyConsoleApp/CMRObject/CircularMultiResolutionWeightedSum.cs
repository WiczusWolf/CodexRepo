using System.Numerics;
using static MyConsoleApp.IntMath;
using static MyConsoleApp.INumberMath;

namespace MyConsoleApp.CMRObject
{
    public class CircularMultiResolutionWeightedSum<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        private readonly ICMRObject<T> _src;
        private int _xCounter = 0;
        private T _runningWeightedSum = T.Zero;
        private T _runningSum = T.Zero;
        private readonly T _runningWeightedSumMaxBeforeReset;

        protected readonly T[][] _runningYSums;
        protected readonly T[] _removedYSums;
        protected readonly T[][] _runningXYSums;
        protected readonly T[] _removedXYSums;

        public CircularMultiResolutionWeightedSum(ICMRObject<T> src, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedMaxItemValue = 5000)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
            _src = src;
            // anticipate larger values due to weighting by x
            _runningWeightedSumMaxBeforeReset = T.CreateTruncating(anticipatedMaxItemValue * _maxSize * _maxSize * _maxSize * 2d);

            _removedYSums = new T[_partitionCount];
            _removedXYSums = new T[_partitionCount];
            _runningYSums = new T[_partitionCount][];
            _runningXYSums = new T[_partitionCount][];
            for (int i = 0; i < _partitionCount; i++)
            {
                _runningYSums[i] = new T[_partitionSize];
                _runningXYSums[i] = new T[_partitionSize];
            }


            src.SubscribeValueAdded(OnPushFront);
        }

        protected override void Assign(int realPartitionIndex, int realItemIndex)
        {
            _removedYSums[realPartitionIndex] = _runningYSums[realPartitionIndex][realItemIndex];
            _runningYSums[realPartitionIndex][_cursors[realPartitionIndex]] = _runningSum;

            _removedXYSums[realPartitionIndex] = _runningXYSums[realPartitionIndex][realItemIndex];
            _runningXYSums[realPartitionIndex][realItemIndex] = _runningWeightedSum;
        }
        protected override void AssignFirst(T value, int realItemIndex)
        {
            _runningSum += value;
            _runningWeightedSum += T.CreateTruncating(_xCounter) * value;
            _xCounter++;

            _removedYSums[0] = _runningYSums[0][realItemIndex];
            _runningYSums[0][_cursors[0]] = _runningSum;

            _removedXYSums[0] = _runningXYSums[0][realItemIndex];
            _runningXYSums[0][realItemIndex] = _runningWeightedSum;
        }
        protected override void PostItemPush()
        {
            if (_runningSum >= _runningWeightedSumMaxBeforeReset)
            {
                ApplyRemoved();
            }
        }

        private void OnPushFront() => PushFront(_src.First());


        public void ApplyRemoved()
        {
            T currentSXYWindowDelta = GetWithNonCircularItemIndex(_runningXYSums, _partitionCount - 1, _partitionSize - 1);
            T currentSYWindowDelta = GetWithNonCircularItemIndex(_runningYSums, _partitionCount - 1, _partitionSize - 1);
            for (int i = 0; i < _partitionCount; i++)
            {
                for (int j = 0; j < _partitionSize; j++)
                {
                    T currentSXY = _runningXYSums[i][j];
                    T currentSY = _runningYSums[i][j];
                    T currentSXYWindow = currentSXY - currentSXYWindowDelta;
                    T currentSYWindow = currentSY - currentSYWindowDelta;
                    T weightAdjustedSYWindow = currentSYWindow * T.CreateTruncating(FastMax(0, _xCounter - _maxSize));
                    _runningXYSums[i][j] = currentSXYWindow - weightAdjustedSYWindow;
                    _runningYSums[i][j] -= _removedYSums[_partitionCount - 1];
                }
            }

            for (int i = 0; i < _partitionCount; i++)
            {
                _removedXYSums[i] = T.Zero;
                _removedYSums[i] = T.Zero;
            }
            _runningWeightedSum = GetWithNonCircularItemIndex(_runningXYSums, 0, 0);
            _runningSum = GetWithNonCircularItemIndex(_runningYSums, 0, 0);
            _xCounter = _maxSize;
        }

        public override T First()
        {

            T currentSXY = GetWithNonCircularItemIndex(_runningXYSums, 0, 0);
            T currentSY = GetWithNonCircularItemIndex(_runningYSums, 0, 0);
            T currentSXYWindow = currentSXY - _removedXYSums[_partitionCount - 1];
            T currentSYWindow = currentSY - _removedYSums[_partitionCount - 1];
            T weightAdjustedSYWindow = currentSYWindow * T.CreateTruncating(FastMax(0, _xCounter - _maxSize));
            return currentSXYWindow - weightAdjustedSYWindow;
        }

        public override T this[CMRIndex index]
        {
            get
            {
                int partitionIndex = index.PartitionIndex;
                int itemIndex = index.ItemIndex;
                int itemOffset = index.Offset;

                T currentSXY = GetWithNonCircularItemIndex(_runningXYSums, partitionIndex, itemIndex);
                T nextSXY = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1,
                    GetWithNonCircularItemIndex(_runningXYSums, partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)),
                    _removedXYSums[partitionIndex]);
                T previousSXY = GetWithNonCircularItemIndex(_runningXYSums, partitionIndex, itemIndex - 1);

                T currentSY = GetWithNonCircularItemIndex(_runningYSums, partitionIndex, itemIndex);
                T nextSY = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1,
                    GetWithNonCircularItemIndex(_runningYSums, partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)),
                    _removedYSums[partitionIndex]);
                T previousSY = GetWithNonCircularItemIndex(_runningYSums, partitionIndex, itemIndex - 1);

                var (offset, maxOffset) = ComputeOffsetFromPartitionEnd(partitionIndex, itemOffset);

                T interpSXY = Interpolate(currentSXY, previousSXY, nextSXY, offset, maxOffset);
                T interpSY = Interpolate(currentSY, previousSY, nextSY, offset, maxOffset);

                T currentSXYWindow = interpSXY - _removedXYSums[_partitionCount - 1];
                T currentSYWindow = interpSY - _removedYSums[_partitionCount - 1];
                T weightAdjustedSYWindow = currentSYWindow * T.CreateTruncating(FastMax(0, _xCounter - _maxSize));
                return currentSXYWindow - weightAdjustedSYWindow;
            }
        }

    }
}
