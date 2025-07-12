using System.Numerics;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp
{
    public class CircularMultiResolutionArray<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        protected readonly T[][] _partitions;
        protected readonly T[] _removed;
        public CircularMultiResolutionArray(int partitionCount, int partitionSize, int magnitudeIncrease)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
            _removed = new T[_partitionCount];
            _partitions = new T[_partitionCount][];
            for (int i = 0; i < _partitionCount; i++)
            {
                _partitions[i] = new T[_partitionSize];
            }
        }

        public override T First() => GetWithNonCircularItemIndex(_partitions, 0, 0);

        public void PushFront(T value)
        {
            _partitions[0][_cursors[0]] = value;
            _cursors[0] = (_cursors[0] + 1) % _partitionSize;
            IncrementModuloCount();

            for (int i = 1; i < _partitionCount; i++)
            {
                if (_countModLast % _modulos[i] == 0)
                {
                    _removed[i] = _partitions[i][_cursors[i]];
                    _partitions[i][_cursors[i]] = AverageFromPartition(i - 1);
                    _cursors[i] = (_cursors[i] + 1) % _partitionSize;
                }
                else
                {
                    break;
                }
            }

            AdvanceCounters(0);
            OnValueAdded.Invoke();
        }

        protected override (int offset, int maxOffset) ComputeOffset(int partitionIndex, int itemOffset)
        {
            int maxOffset = _modulos[partitionIndex];
            int selectedOffset = QuadraticOffset(itemOffset, maxOffset) - _offsets[partitionIndex] * 2;
            return (selectedOffset, maxOffset * 2);
        }

        private T AverageFromPartition(int partitionIndex)
        {
            int realItemIndex = (_cursors[partitionIndex]) & _partitionSizeMask;
            T sum = T.Zero;
            for (int i = 0; i < _magnitudeIncrease; i++)
            {
                realItemIndex = (realItemIndex - 1) & _partitionSizeMask;
                sum += _partitions[partitionIndex][realItemIndex];
            }

            return sum / T.CreateTruncating(_magnitudeIncrease);
        }

        public override T this[CMRIndex index]
        {
            get
            {

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
                    return Interpolate(current, previous, next, offset, maxOffset);
                }
            }
        }
    }
}
