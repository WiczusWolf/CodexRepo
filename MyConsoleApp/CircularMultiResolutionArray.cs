using Common.Delegates;
using System.Numerics;
using System.Text;

namespace MyConsoleApp
{
    public partial class CircularMultiResolutionArray<T> where T : INumber<T>
    {
        private readonly int _partitions;
        private readonly int _size;
        private readonly int _increase;
        private readonly T[][] _arrays;
        private readonly int[] _starts;
        private readonly T[] _sums;
        private readonly int[] _counts;
        private readonly int[] _filledCounts;
        private readonly int _maxCount;
        private int _count;
        public readonly EventHandlerSync<T>[] OnValueAdded;
        public readonly EventHandlerSync<T>[] OnValueRemoved;

        public int MaxCount => _maxCount;
        public int Count => _count;
        public int Partitions => _partitions;
        public int Size => _size;
        public int Increase => _increase;

        private int Pow(int exponent)
        {
            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= _increase;
            }
            return result;
        }

        public IndexInfo GetIndex(int naiveIndex)
        {
            if (naiveIndex < 0 || naiveIndex >= _maxCount)
                throw new ArgumentOutOfRangeException(nameof(naiveIndex));


            for (int partitionIndex = 0; partitionIndex < _partitions; partitionIndex++)
            {
                int factor = Pow(partitionIndex);
                int itemIndex = naiveIndex / factor;
                if (itemIndex < _size)
                {
                    int offset = naiveIndex % factor;
                    return new IndexInfo(partitionIndex, itemIndex, offset);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(naiveIndex));
        }

        public CircularMultiResolutionArray(int partitions, int size, int increase)
        {
            if (partitions <= 0) throw new ArgumentOutOfRangeException(nameof(partitions));
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (increase <= 0) throw new ArgumentOutOfRangeException(nameof(increase));

            _partitions = partitions;
            _size = size;
            _increase = increase;
            _arrays = new T[partitions][];
            _starts = new int[partitions];
            _sums = new T[partitions];
            _counts = new int[partitions];
            _filledCounts = new int[partitions];
            for (int i = 1; i < partitions; i++) size *= increase;
            _maxCount = size;
            OnValueAdded = new EventHandlerSync<T>[partitions];
            OnValueRemoved = new EventHandlerSync<T>[partitions];

            for (int i = 0; i < partitions; i++)
            {
                _arrays[i] = new T[size];
                _starts[i] = 0;
                _sums[i] = T.Zero;
                _counts[i] = 0;
                _filledCounts[i] = 0;
                OnValueAdded[i] = new EventHandlerSync<T>();
                OnValueRemoved[i] = new EventHandlerSync<T>();
            }
        }

        public void PushFront(T item)
        {
            PushToLevel(0, item);
            _count = (_count + 1) % _maxCount;
        }

        private void PushToLevel(int level, T item)
        {
            if (level >= _partitions)
                return;

            int start = (_starts[level] - 1 + _size) % _size;
            T removedValue = _arrays[level][start];
            bool removed = false;

            if (_filledCounts[level] == _size)
            {
                removed = true;
            }
            else
            {
                _filledCounts[level]++;
            }

            _starts[level] = start;
            _arrays[level][start] = item;

            _sums[level] += item;
            _counts[level]++;

            if (_counts[level] >= _increase)
            {
                T avg = _sums[level] / T.CreateChecked(_increase);
                _sums[level] = T.Zero;
                _counts[level] = 0;
                PushToLevel(level + 1, avg);
            }

            OnValueAdded[level].Invoke(item);
            if (removed)
            {
                OnValueRemoved[level].Invoke(removedValue);
            }
        }

        public T this[int partition, int index]
        {
            get
            {
                if (partition < 0 || partition >= _partitions)
                    throw new ArgumentOutOfRangeException(nameof(partition));
                if (index < 0 || index >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                int realIndex = (_starts[partition] + index) % _size;
                return _arrays[partition][realIndex];
            }
        }

        public T this[int naiveIndex]
        {
            get
            {
                var info = GetIndex(naiveIndex);
                return this[info];
            }
        }

        public T this[IndexInfo index]
        {
            get
            {
                if (index.PartitionIndex < 0 || index.PartitionIndex >= _partitions)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index.ItemIndex < 0 || index.ItemIndex >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));

                int realIndex = (_starts[index.PartitionIndex] + index.ItemIndex) % _size;
                return _arrays[index.PartitionIndex][realIndex];
            }
        }

        public int GetStartIndex(int partition)
        {
            if (partition < 0 || partition >= _partitions)
                throw new ArgumentOutOfRangeException(nameof(partition));
            return _starts[partition];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < _count; i++)
            {
                sb.Append(this[i]);
                sb.Append(", ");
            }
            if (sb.Length > 2)
            {
                sb.Length -= 2;
                sb.Append("]");
            }
            return sb.ToString();
        }
    }
}
