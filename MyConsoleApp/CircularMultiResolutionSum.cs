using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static MyConsoleApp.IntMath;
namespace MyConsoleApp
{
    internal class CircularMultiResolutionSum<T> where T : INumber<T>
    {
        public int Count => _count;
        public int MaxSize => _maxSize;
        public int PartitionSize => _partitionSize;
        public int MagnitudeIncrease => _magnitudeIncrease;
        public int PartitionCount => _partitionCount;
        public IReadOnlyCollection<T>[] Partitions => _partitions.Select(p => p.AsReadOnly()).ToArray();

        public EventHandlerSync<T> OnValueAdded = new();

        CircularMultiResolutionArray<T> _src;
        private T[][] _partitions;
        private T[] _removed;
        private int[] _cursors;
        private int[] _modulos;
        private int[] _offsets;
        private int _count;
        private int _maxSize;
        private int _partitionCount;
        private int _partitionSize;
        private int _magnitudeIncrease;
        private int _partitionLog;
        private int _countModLast;
        private int _partitionSizeMask;
        private T _runningSumMaxBeforeReset;
        private T _runningSum = T.Zero;
        public CircularMultiResolutionSum(CircularMultiResolutionArray<T> src, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedItemValue = 5000)
        {
            if (!partitionSize.IsPowerOfTwo() || partitionSize <= 0)
                throw new ArgumentException($"Partition Size must be a power of 2, got {_partitionSize}.");
            if (!magnitudeIncrease.IsPowerOfTwo() || magnitudeIncrease <= 1)
                throw new ArgumentException($"Magnitude step must be a power of 2, got {_magnitudeIncrease}.");
            if (magnitudeIncrease >= partitionSize)
                throw new ArgumentException($"Magnitude step {_magnitudeIncrease} must be greater than partition size {_partitionSize}.");

            _src = src;
            _partitionCount = partitionCount;
            _partitionSize = partitionSize;
            _magnitudeIncrease = magnitudeIncrease;

            _partitionLog = BitOperations.Log2((uint)magnitudeIncrease);
            _partitionSizeMask = _partitionSize - 1;
            _maxSize = (_partitionSize - 1) * Pow(magnitudeIncrease, partitionCount - 1) - 1;
            _partitions = new T[_partitionCount][];
            _cursors = new int[_partitionCount];
            _removed = new T[_partitionCount];
            _offsets = new int[_partitionCount];
            _modulos = new int[_partitionCount];
            _modulos[0] = 1;
            _runningSumMaxBeforeReset = T.CreateChecked(_maxSize * anticipatedItemValue);
            for (int i = 1; i < _partitionCount; i++)
            {
                _modulos[i] = _modulos[i - 1] * _magnitudeIncrease;
            }
            for (int i = 0; i < _partitionCount; i++)
            {
                _partitions[i] = new T[_partitionSize];
            }

            src.OnValueAdded[0].Add(OnPushFront);
        }
        public T First() => GetWithNonCircularItemIndex(0, 0);

        private void OnPushFront(T value)
        {
            _runningSum += value;
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

            _count = Math.Min(_count + 1, _maxSize);
            _countModLast = (_countModLast + 1) % _modulos[_partitionCount - 1];

            for (int i = 0; i < _partitionCount; i++)
            {
                _offsets[i] = (_modulos[i] + _countModLast - 1) % _modulos[i];
            }
            OnValueAdded.Invoke(_runningSum - _removed[_partitionCount - 1]);
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
        public T this[CMRSIndex index]
        {
            get
            {
                int partitionIndex = index.PartitionIndex;
                int itemIndex = index.ItemIndex;
                int itemOffset = index.Offset - _offsets[partitionIndex];

                T current = GetWithNonCircularItemIndex(partitionIndex, itemIndex);
                T next = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1, GetWithNonCircularItemIndex(partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)), _removed[partitionIndex]);
                T previous = GetWithNonCircularItemIndex(partitionIndex, itemIndex - 1);

                return Interpolate(current, previous, next, itemOffset, _modulos[partitionIndex]) - _removed[_partitionCount - 1];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T SwitchOnGreaterOrEqualZero(int comparator, T onLower, T onGreater)
        {
            int isGE = ~(comparator >> 31) & 1;
            T flag = T.CreateTruncating(isGE);
            return onLower + (onGreater - onLower) * flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T Interpolate(T current, T previous, T next, int offset, int maxOffset)
        {
            int absOff = FastAbs(offset);
            var invMax = T.One / T.CreateTruncating(maxOffset);

            T frac = T.CreateTruncating(absOff) * invMax;

            T currFrac = T.One - frac;
            return current * currFrac + SwitchOnGreaterOrEqualZero(offset, frac * previous, frac * next);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetWithNonCircularItemIndex(int partitionIndex, int nonCircularIndex)
        {
#if SAFE
            int realItemIndex = (_cursors[partitionIndex] - nonCircularIndex - 1) & _partitionSizeMask;
            return _partitions[partitionIndex][realItemIndex];
#else
            unsafe
            {
                int realItemIndex = _cursors[partitionIndex] - nonCircularIndex - 1 & _partitionSizeMask;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                fixed (T* arr = _partitions[partitionIndex])
                {
                    return arr[realItemIndex];
                }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
            }
#endif
        }
        public CMRSIndex GetIndex(uint index)
        {
            if (index > _maxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be in range 0 to {_maxSize}, got {index}.");
            }
            uint partitionIndex = index / (uint)_partitionSize;
            if (partitionIndex > 0)
            {
                partitionIndex = (uint)BitOperations.Log2(partitionIndex) / (uint)_partitionLog + 1;
            }
            int itemIndex = (int)index / _modulos[partitionIndex];
            int offset = (int)index % _modulos[partitionIndex];
            return new CMRSIndex((int)partitionIndex, itemIndex, offset);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (uint i = 0; i < _count; i++)
            {
                sb.Append(this[GetIndex(i)]);
                sb.Append(", ");
            }
            if (sb.Length > 2) sb.Length -= 2;
            sb.Append("]");
            return sb.ToString();
        }
    }
}
