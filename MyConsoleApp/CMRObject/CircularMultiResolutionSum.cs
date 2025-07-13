using System.Numerics;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp
{
    public class CircularMultiResolutionSum<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        private readonly ICMRObject<T> _src;
        private T _runningSum = T.Zero;
        protected T _runningSumMaxBeforeReset;
        protected readonly T[][] _partitions;
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
            _partitions = new T[_partitionCount][];
            for (int i = 0; i < _partitionCount; i++)
            {
                _partitions[i] = new T[_partitionSize];
            }

            src.SubscribeValueAdded(OnPushFront);
        }
        public override T First() => GetWithNonCircularItemIndex(_partitions, 0, 0) - _removed[_partitionCount - 1];
        private void OnPushFront()
        {
            _runningSum += _src.First();
            for (int i = 0; i < _partitionCount; i++)
            {
                if (_countModLast % _modulos[i] == 0)
                {
                    _removed[i] = _partitions[i][_cursors[i]];
                    _partitions[i][_cursors[i]] = _runningSum;
                    _cursors[i] = (_cursors[i] + 1) % _partitionSize;
                }
                else
                {
                    break;
                }
            }

            IncrementModuloCount();
            AdvanceCounters(-1);
            if (_runningSum >= _runningSumMaxBeforeReset)
            {
                ApplyRemoved();
            }
            OnValueAdded.Invoke();
        }
        private void ApplyRemoved()
        {
            for (int i = 0; i < _partitionCount; i++)
            {
                for (int j = 0; j < _partitionSize; j++)
                {
                    _partitions[i][j] -= _removed[_partitionSize - 1];
                }
            }
            _removed[_partitionSize - 1] = T.Zero;
        }
        protected override (int offset, int maxOffset) ComputeOffset(int partitionIndex, int itemOffset)
        {
            int selectedOffset = itemOffset - _offsets[partitionIndex];
            return (selectedOffset, _modulos[partitionIndex]);
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

                var (offset, maxOffset) = ComputeOffset(partitionIndex, itemOffset);
                return Interpolate(current, previous, next, offset, maxOffset) - _removed[_partitionCount - 1];

            }
        }
    }
}
