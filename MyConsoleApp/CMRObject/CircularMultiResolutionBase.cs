using System.Numerics;
using System.Text;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp.CMRObject
{
    public abstract class CircularMultiResolutionBase<T> : ICMRObject<T> where T : INumber<T>
    {
        public int Count => _count;
        public int MaxSize => _maxSize;
        public int PartitionSize => _partitionSize;
        public int MagnitudeIncrease => _magnitudeIncrease;
        public int PartitionCount => _partitionCount;
        //public IReadOnlyCollection<T>[] Partitions => _partitions.Select(p => p.AsReadOnly()).ToArray();

        protected readonly EventHandlerSync OnValueAdded = new();

        protected readonly int[] _cursors;
        protected readonly int[] _modulos;
        protected readonly int[] _offsets;
        protected readonly int _maxSize;
        protected readonly int _partitionCount;
        protected readonly int _partitionSize;
        protected readonly int _magnitudeIncrease;
        protected readonly int _partitionLog;
        protected readonly int _partitionSizeMask;
        protected int _modulatedItemCount;
        protected int _count;

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
            _cursors = new int[_partitionCount];
            _offsets = new int[_partitionCount];
            _modulos = new int[_partitionCount];
            _modulos[0] = 1;
            for (int i = 1; i < _partitionCount; i++)
            {
                _modulos[i] = _modulos[i - 1] * _magnitudeIncrease;
            }
        }

        public void SubscribeValueAdded(Action action) => OnValueAdded.Add(action);
        public void UnsubscribeValueAdded(Action action) => OnValueAdded.Remove(action);

        public abstract T First();
        public abstract T this[CMRIndex index] { get; }
        protected abstract void AssignFirst(T value, int realItemIndex);
        protected abstract void Assign(int realPartitionIndex, int realItemIndex);
        protected abstract void PostItemPush();

        public void PushFront(T value)
        {
            AssignFirst(value, _cursors[0]);
            AdvanceCounters();
            _cursors[0] = (_cursors[0] + 1) % _partitionSize;

            for (int i = 1; i < _partitionCount; i++)
            {
                if (_modulatedItemCount % _modulos[i] == 0)
                {
                    Assign(i, _cursors[i]);
                    _cursors[i] = (_cursors[i] + 1) % _partitionSize;
                }
                else
                {
                    break;
                }
            }
            PostItemPush();
            OnValueAdded.Invoke();
        }
        public CMRIndex GetIndex(int index)
        {
            if (index > _maxSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be in range 0 to {_maxSize}, got {index}.");
            }
            int partitionIndex = index / _partitionSize;
            if (partitionIndex == 0) return new CMRIndex(0, index, 0);
            if (partitionIndex > 0)
            {
                partitionIndex = BitOperations.Log2((uint)partitionIndex) / _partitionLog + 1;
            }
            int itemC = index;
            //int itemC = index + 1 - _modulos[partitionIndex];
            int itemIndex = (itemC / _modulos[partitionIndex]);
            int offset = itemC % _modulos[partitionIndex];
            return new CMRIndex(partitionIndex, itemIndex, offset);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < _count; i++)
            {
                sb.Append(this[GetIndex(i)]);
                sb.Append(", ");
            }
            if (sb.Length > 2) sb.Length -= 2;
            sb.Append(']');
            return sb.ToString();
        }
        protected T GetWithNonCircularItemIndex(T[][] src, int partitionIndex, int nonCircularIndex)
        {
            int currentFirstIndex = _cursors[partitionIndex] - 1;
            int invertedIndex = _partitionSize - nonCircularIndex;
            int realItemIndex = (currentFirstIndex - nonCircularIndex) & _partitionSizeMask;
            return src[partitionIndex][realItemIndex];
        }

        protected (int offset, int maxOffset) ComputeOffsetFromHalfPartition(int partitionIndex, int itemOffset)
        {
            if (partitionIndex == 0) return (itemOffset, _modulos[partitionIndex]);

            int maxOffset = _modulos[partitionIndex];
            int offset = _cursors[partitionIndex] % maxOffset;
            int selectedOffset = QuadraticOffset(itemOffset, maxOffset) - maxOffset * 2;
            return (selectedOffset, maxOffset * 2);
        }
        protected (int offset, int maxOffset) ComputeOffsetFromPartitionEnd(int partitionIndex, int itemOffset)
        {
            return (itemOffset - _offsets[partitionIndex], _modulos[partitionIndex]);//?
        }

        private void AdvanceCounters()
        {
            _count = Math.Min(_count + 1, _maxSize);
            for (int i = 0; i < _partitionCount; i++)
            {
                _offsets[i] = (_offsets[i] + 1) % _modulos[i];
            }
            _modulatedItemCount = (_modulatedItemCount + 1) % _maxSize;
        }
    }
}
