using System.Numerics;

namespace MyConsoleApp
{
    public class CircularMultiResolutionArray<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        public CircularMultiResolutionArray(int partitionCount, int partitionSize, int magnitudeIncrease)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
        }


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
    }
}
