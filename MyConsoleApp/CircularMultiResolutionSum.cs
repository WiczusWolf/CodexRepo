using System.Numerics;
using static MyConsoleApp.IntMath;

namespace MyConsoleApp
{
    internal class CircularMultiResolutionSum<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        private readonly CircularMultiResolutionArray<T> _src;
        private T _runningSum = T.Zero;

        public CircularMultiResolutionSum(CircularMultiResolutionArray<T> src, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedItemValue = 5000)
            : base(partitionCount, partitionSize, magnitudeIncrease, anticipatedItemValue)
        {
            _src = src;
            src.OnValueAdded.Add(OnPushFront);
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

            IncrementModuloCount();
            AdvanceCounters(-1);
            OnValueAdded.Invoke(_runningSum - _removed[_partitionCount - 1]);
        }

        protected override (int offset, int maxOffset) ComputeOffset(int partitionIndex, int itemOffset)
        {
            int selectedOffset = itemOffset - _offsets[partitionIndex];
            return (selectedOffset, _modulos[partitionIndex]);
        }

        protected override T PostProcess(T value) => value - _removed[_partitionCount - 1];
    }
}
