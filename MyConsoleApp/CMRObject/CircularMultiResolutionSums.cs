using System.Numerics;
using static MyConsoleApp.INumberMath;

namespace MyConsoleApp.CMRObject
{
    public class CircularMultiResolutionSums<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        private readonly ICMRObject<T> _src;
        private T _runningSum = T.Zero;
        protected T _runningSumMaxBeforeReset;
        protected readonly T[][] _runningSums;
        protected readonly T[] _removed;
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="src">Items added to this array will contribute to the running sum. </param>
        /// <param name="anticipatedMaxItemValue">The sum uses variables that increase indefinetely, they have to be reset sometimes. This value should be around average what is added to the array. </param>
        public CircularMultiResolutionSums(ICMRObject<T> src, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedMaxItemValue = 5000)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
            _src = src;
            _runningSumMaxBeforeReset = T.CreateTruncating(anticipatedMaxItemValue * _maxSize * 2);
            _removed = new T[_partitionCount];
            _runningSums = new T[_partitionCount][];
            for (int i = 0; i < _partitionCount; i++)
            {
                _runningSums[i] = new T[_partitionSize];
            }

            src.SubscribeValueAdded(OnPushFront);
        }
        public override T First() => GetWithNonCircularItemIndex(_runningSums, 0, 0) - _removed[_partitionCount - 1];
        protected override void Assign(int realPartitionIndex, int realItemIndex)
        {
            _removed[realPartitionIndex] = _runningSums[realPartitionIndex][realItemIndex];
            _runningSums[realPartitionIndex][_cursors[realPartitionIndex]] = _runningSum;
        }

        protected override void AssignFirst(T value, int realItemIndex)
        {
            _runningSum += value;
            _removed[0] = _runningSums[0][realItemIndex];
            _runningSums[0][realItemIndex] = _runningSum;
        }
        protected override void PostItemPush()
        {
            if (_runningSum >= _runningSumMaxBeforeReset)
            {
                ApplyRemoved();
            }
        }
        private void OnPushFront() => PushFront(_src.First());
        private void ApplyRemoved()
        {
            for (int i = 0; i < _partitionCount; i++)
            {
                for (int j = 0; j < _partitionSize; j++)
                {
                    _runningSums[i][j] -= _removed[_partitionSize - 1];
                }
            }
            _removed[_partitionSize - 1] = T.Zero;
        }

        public override T this[CMRIndex index]
        {
            get
            {
                int partitionIndex = index.PartitionIndex;
                int itemIndex = index.ItemIndex;
                int itemOffset = index.Offset;

                CMRIndex currentIndex = CurrentIndex(index);
                CMRIndex olderIndex = OlderIndex(index);

                T current = GetWithNonCircularItemIndex(_runningSums, currentIndex.PartitionIndex, currentIndex.ItemIndex);
                T older = GetThisOrRemoved(_runningSums, _removed, olderIndex);

                T removed = _removed[_partitionCount - 1];
                T last = GetWithNonCircularItemIndex(_runningSums, _partitionCount - 1, _partitionSize - 1);
                T delta = last - removed;
                T deltaOffsetAdjusted = delta * T.CreateTruncating(_offsets[_partitionCount - 1]) / T.CreateTruncating(_modulos[_partitionCount - 1]);
                T toRemove = removed + deltaOffsetAdjusted;
                //toRemove = T.Zero;
                T result = Interpolate(current, older, currentIndex.Offset, currentIndex.Modulo) - toRemove;
                return result;
            }
        }
    }
}
