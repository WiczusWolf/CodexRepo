using System.Numerics;

namespace MyConsoleApp
{
    public class CircularMultiResolutionWeightedSum<T> : CircularMultiResolutionBase<T> where T : INumber<T>
    {
        private readonly ICMRObject<T> _src;
        private long _xCounter = 0;
        private T _runningWeightedSum = T.Zero;
        private T _runningSum = T.Zero;
        private readonly T _runningWeightedSumMaxBeforeReset;

        public CircularMultiResolutionWeightedSum(ICMRObject<T> src, int partitionCount, int partitionSize, int magnitudeIncrease, double anticipatedMaxItemValue = 5000)
            : base(partitionCount, partitionSize, magnitudeIncrease)
        {
            _src = src;
            // anticipate larger values due to weighting by x
            _runningWeightedSumMaxBeforeReset = T.CreateTruncating(anticipatedMaxItemValue * _maxSize * _maxSize * 2d);
            src.SubscribeValueAdded(OnPushFront);
        }

        private void OnPushFront()
        {
            _xCounter++;
            T value = _src.First();
            _runningSum += value;
            _runningWeightedSum += T.CreateTruncating(_xCounter) * value;

            for (int i = 0; i < _partitionCount; i++)
            {
                if (_countModLast % _modulos[i] == 0)
                {
                    _removed[i] = _partitions[i][_cursors[i]];
                    _partitions[i][_cursors[i]] = _runningWeightedSum;
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

            ResetCounterIfNeeded();

            OnValueAdded.Invoke();
        }

        private void ApplyRemoved()
        {
            T removedBase = _removed[_partitionCount - 1];
            for (int i = 0; i < _partitionCount; i++)
            {
                for (int j = 0; j < _partitionSize; j++)
                {
                    _partitions[i][j] -= removedBase;
                }
            }
            _removed[_partitionCount - 1] = T.Zero;
        }

        protected override (int offset, int maxOffset) ComputeOffset(int partitionIndex, int itemOffset)
        {
            int selectedOffset = itemOffset - _offsets[partitionIndex];
            return (selectedOffset, _modulos[partitionIndex]);
        }

        protected override T PostProcess(T value) => value - _removed[_partitionCount - 1];

        private void ResetCounterIfNeeded()
        {
            if (_xCounter < _maxSize)
                return;

            T correction = T.CreateTruncating(_maxSize) * _runningSum;
            _runningWeightedSum -= correction;
            for (int i = 0; i < _partitionCount; i++)
            {
                _removed[i] -= correction;
                for (int j = 0; j < _partitionSize; j++)
                {
                    _partitions[i][j] -= correction;
                }
            }
            _xCounter -= _maxSize;
        }
    }
}
