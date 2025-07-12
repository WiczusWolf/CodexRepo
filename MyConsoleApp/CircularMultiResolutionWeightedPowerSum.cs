using System.Numerics;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp
{
    public class CircularMultiResolutionWeightedPowerSum<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        private readonly ICMRObject<T> _src;
        private readonly int _power;
        private int _xCounter = 0;
        private T _runningWeightedSum = T.Zero;
        private T _runningSum = T.Zero;
        private readonly T _runningWeightedSumMaxBeforeReset;

        protected readonly T[][] _runningYSums;
        protected readonly T[] _removedYSums;
        protected readonly T[][] _runningXYSums;
        protected readonly T[] _removedXYSums;

        public CircularMultiResolutionWeightedPowerSum(ICMRObject<T> src, int power, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedMaxItemValue = 5000)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
            if (power < 1) throw new ArgumentException("power must be >= 1", nameof(power));

            _src = src;
            _power = power;
            _runningWeightedSumMaxBeforeReset = T.CreateTruncating(anticipatedMaxItemValue * Pow(_maxSize, power + 2) * 2d);

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

        private static int Pow(int value, int pow)
        {
            int result = 1;
            for (int i = 0; i < pow; i++) result *= value;
            return result;
        }

        private void OnPushFront()
        {
            T value = _src.First();

            _runningSum += value;
            T weight = T.CreateTruncating(Pow(_xCounter, _power));
            _runningWeightedSum += weight * value;

            _xCounter++;

            for (int i = 0; i < _partitionCount; i++)
            {
                if (_countModLast % _modulos[i] == 0)
                {
                    _removedYSums[i] = _runningYSums[i][_cursors[i]];
                    _runningYSums[i][_cursors[i]] = _runningSum;

                    _removedXYSums[i] = _runningXYSums[i][_cursors[i]];
                    _runningXYSums[i][_cursors[i]] = _runningWeightedSum;

                    _cursors[i] = (_cursors[i] + 1) % _partitionSize;
                }
                else
                {
                    break;
                }
            }

            IncrementModuloCount();
            AdvanceCounters(-1);

            if (_runningWeightedSum >= _runningWeightedSumMaxBeforeReset)
            {
                ApplyRemoved();
            }

            OnValueAdded.Invoke();
        }

        protected override (int offset, int maxOffset) ComputeOffset(int partitionIndex, int itemOffset)
        {
            int maxOffset = _modulos[partitionIndex];
            int selectedOffset = QuadraticOffset(itemOffset, maxOffset) - _offsets[partitionIndex] * 2;
            return (selectedOffset, maxOffset * 2);
        }

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

                var (offset, maxOffset) = ComputeOffset(partitionIndex, itemOffset);

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
