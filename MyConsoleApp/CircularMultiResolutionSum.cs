using System.Numerics;
using System.Text;

namespace MyConsoleApp
{
    public class CircularMultiResolutionSum<T> where T : INumber<T>
    {
        private readonly CircularMultiResolutionArray<T> _src;
        private readonly int _partitions;
        private readonly int _size;
        private readonly int _increase;
        private readonly int _maxCount;

        private readonly T[][] _runningSums;
        private readonly int?[] _pendingIndices;

        private T _runningTotal;
        private T _correction;
        private int _count;

        public int MaxCount => _maxCount;
        public int Count => _count;

        public CircularMultiResolutionSum(CircularMultiResolutionArray<T> src)
        {
            _src = src ?? throw new ArgumentNullException(nameof(src));
            _partitions = src.Partitions;
            _size = src.Size;
            _increase = src.Increase;
            _maxCount = _src.MaxCount;

            _runningSums = new T[_partitions][];
            for (int i = 0; i < _partitions; i++)
            {
                _runningSums[i] = new T[_size];
            }

            _pendingIndices = new int?[_partitions];
            _runningTotal = T.Zero;
            _correction = T.Zero;

            for (int p = 0; p < _partitions; p++)
            {
                int level = p; // capture for closure
                _src.OnValueAdded[p].Add(v => OnValueAdded(level, v));
                _src.OnValueRemoved[p].Add(v => OnValueRemoved(level, v));
            }
        }

        private T Pow(int exponent)
        {
            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= _increase;
            }
            return T.CreateChecked(result);
        }

        private int PowInt(int exponent)
        {
            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= _increase;
            }
            return result;
        }

        private void OnValueAdded(int level, T value)
        {
            int start = _src.GetStartIndex(level);

            if (level == 0)
            {
                _runningTotal += value;
                _runningSums[0][start] = _runningTotal;

                for (int l = 1; l < _partitions; l++)
                {
                    if (_pendingIndices[l].HasValue)
                    {
                        _runningSums[l][_pendingIndices[l]!.Value] = _runningTotal;
                        _pendingIndices[l] = null;
                    }
                }
            }
            else
            {
                _pendingIndices[level] = start;
            }
            _count = (_count + 1) % _maxCount;
        }

        private void OnValueRemoved(int level, T value)
        {
            if (level == _partitions - 1)
            {
                _correction += value * Pow(level);
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _partitions * _size)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var info = _src.GetIndex(index + 1); // array uses 1-based naive index

                int partition = info.Partition;
                int partIndex = info.PartitionIndex;
                int offset = info.Offset;

                int start = _src.GetStartIndex(partition);
                T runCurrent = _runningSums[partition][(start + partIndex) % _size];

                T nextRun;
                if (index + 1 < _partitions * _size)
                {
                    var nextInfo = _src.GetIndex(index + 2);
                    int nextStart = _src.GetStartIndex(nextInfo.Partition);
                    nextRun = _runningSums[nextInfo.Partition][(nextStart + nextInfo.PartitionIndex) % _size];
                }
                else
                {
                    nextRun = _correction;
                }

                T diff = runCurrent - nextRun;
                T fraction = offset == 0 ? T.Zero : (T.CreateChecked(offset) / Pow(partition));

                return runCurrent - diff * fraction - _correction;
            }
        }

        public T this[CircularMultiResolutionArray<T>.IndexInfo index]
        {
            get
            {
                if (index.Partition < 0 || index.Partition >= _partitions)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index.PartitionIndex < 0 || index.PartitionIndex >= _size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                if (index.Offset < 0 || index.Offset >= PowInt(index.Partition))
                    throw new ArgumentOutOfRangeException(nameof(index));

                int naive = index.PartitionIndex * PowInt(index.Partition) + index.Offset;
                return this[naive];
            }
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
