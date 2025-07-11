using System.Numerics;
using System.Text;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp
{
    public abstract class CircularMultiResolutionBase<T> where T : INumber<T>
    {
        public int Count => _count;
        public int MaxSize => _maxSize;
        public int PartitionSize => _partitionSize;
        public int MagnitudeIncrease => _magnitudeIncrease;
        public int PartitionCount => _partitionCount;
        public IReadOnlyCollection<T>[] Partitions => _partitions.Select(p => p.AsReadOnly()).ToArray();

        public EventHandlerSync<T> OnValueAdded = new();

        protected readonly T[][] _partitions;
        protected readonly T[] _removed;
        protected readonly int[] _cursors;
        protected readonly int[] _modulos;
        protected readonly int[] _offsets;
        protected int _count;
        protected readonly int _maxSize;
        protected readonly int _partitionCount;
        protected readonly int _partitionSize;
        protected readonly int _magnitudeIncrease;
        protected readonly int _partitionLog;
        protected int _countModLast;
        protected readonly int _partitionSizeMask;

        protected CircularMultiResolutionBase(int partitionCount, int partitionSize, int magnitudeIncrease)
        {
            if (!partitionSize.IsPowerOfTwo() || partitionSize <= 0)
                throw new ArgumentException($"Partition Size must be a power of 2, got {partitionSize}.");
            if (!magnitudeIncrease.IsPowerOfTwo() || magnitudeIncrease <= 1)
                throw new ArgumentException($"Magnitude step must be a power of 2, got {magnitudeIncrease}.");
            if (magnitudeIncrease >= partitionSize)
                throw new ArgumentException($"Magnitude step {magnitudeIncrease} must be greater than partition size {partitionSize}.");

            _partitionCount = partitionCount;
            _partitionSize = partitionSize;
            _magnitudeIncrease = magnitudeIncrease;

            _partitionLog = BitOperations.Log2((uint)magnitudeIncrease);
            _partitionSizeMask = _partitionSize - 1;
            _maxSize = _partitionSize * Pow(magnitudeIncrease, partitionCount - 1);
            _partitions = new T[_partitionCount][];
            _cursors = new int[_partitionCount];
            _removed = new T[_partitionCount];
            _offsets = new int[_partitionCount];
            _modulos = new int[_partitionCount];
            _modulos[0] = 1;
            for (int i = 1; i < _partitionCount; i++)
            {
                _modulos[i] = _modulos[i - 1] * _magnitudeIncrease;
            }
            for (int i = 0; i < _partitionCount; i++)
            {
                _partitions[i] = new T[_partitionSize];
            }
        }
        public T First() => GetWithNonCircularItemIndex(0, 0);

        protected void IncrementModuloCount() => _countModLast = (_countModLast + 1) % _modulos[_partitionCount - 1];

        protected void AdvanceCounters(int offsetAdjustment)
        {
            _count = Math.Min(_count + 1, _maxSize);
            for (int i = 0; i < _partitionCount; i++)
            {
                _offsets[i] = (_modulos[i] + _countModLast + offsetAdjustment) % _modulos[i];
            }
        }

        protected static int QuadraticOffset(int index, int magnitude) =>
            index < (magnitude >> 1)
                ? -(magnitude - (index << 1) - 1)
                : ((index << 1) - magnitude + 1);

        protected T SwitchOnGreaterOrEqualZero(int comparator, T onLower, T onGreater)
        {
            int isGE = ~(comparator >> 31) & 1;
            T flag = T.CreateTruncating(isGE);
            return onLower + (onGreater - onLower) * flag;
        }

        protected T Interpolate(T current, T previous, T next, int offset, int maxOffset)
        {
            int absOff = FastAbs(offset);
            var invMax = T.One / T.CreateTruncating(maxOffset);

            T frac = T.CreateTruncating(absOff) * invMax;

            T currFrac = T.One - frac;
            return current * currFrac + SwitchOnGreaterOrEqualZero(offset, frac * previous, frac * next);
        }

        protected T GetWithNonCircularItemIndex(int partitionIndex, int nonCircularIndex)
        {
#if SAFE
            int realItemIndex = (_cursors[partitionIndex] - nonCircularIndex - 1) & _partitionSizeMask;
            return _partitions[partitionIndex][realItemIndex];
#else
            unsafe
            {
                int realItemIndex = _cursors[partitionIndex] - nonCircularIndex - 1 & _partitionSizeMask;
#pragma warning disable CS8500
                fixed (T* arr = _partitions[partitionIndex])
                {
                    return arr[realItemIndex];
                }
#pragma warning restore CS8500
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
            sb.Append('[');
            for (uint i = 0; i < _count; i++)
            {
                sb.Append(this[GetIndex(i)]);
                sb.Append(", ");
            }
            if (sb.Length > 2) sb.Length -= 2;
            sb.Append(']');
            return sb.ToString();
        }

        protected abstract (int offset, int maxOffset) ComputeOffset(int partitionIndex, int itemOffset);

        protected virtual T PostProcess(T value) => value;

        private protected T InterpolatedAt(CMRSIndex index)
        {
            int partitionIndex = index.PartitionIndex;
            int itemIndex = index.ItemIndex;
            int itemOffset = index.Offset;

            T current = GetWithNonCircularItemIndex(partitionIndex, itemIndex);
            T next = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1,
                GetWithNonCircularItemIndex(partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)),
                _removed[partitionIndex]);
            T previous = GetWithNonCircularItemIndex(partitionIndex, itemIndex - 1);

            var (offset, maxOffset) = ComputeOffset(partitionIndex, itemOffset);
            return PostProcess(Interpolate(current, previous, next, offset, maxOffset));
        }

        public virtual T this[CMRSIndex index] => InterpolatedAt(index);
    }
}
