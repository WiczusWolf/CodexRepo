using System.Numerics;
using static MyConsoleApp.IntMath;
using static MyConsoleApp.INumberMath;

namespace MyConsoleApp.CMRObject
{
    public class CircularMultiResolutionSum<T> : CircularMultiResolutionBase<T> where T : INumber<T>
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
        public CircularMultiResolutionSum(ICMRObject<T> src, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedMaxItemValue = 5000)
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
            _runningSums[0][realItemIndex] = value;
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

                T current = GetWithNonCircularItemIndex(_runningSums, partitionIndex, itemIndex);
                T next = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1,
                    GetWithNonCircularItemIndex(_runningSums, partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)),
                    _removed[partitionIndex]);
                T previous = GetWithNonCircularItemIndex(_runningSums, partitionIndex, itemIndex - 1);

                var (offset, maxOffset) = ComputeOffsetFromPartitionEnd(partitionIndex, itemOffset);
                return Interpolate(current, previous, next, offset, maxOffset) - _removed[_partitionCount - 1];

            }
        }
    }
}
