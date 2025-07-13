using MyConsoleApp.CMRObject;
using System.Numerics;

namespace MyConsoleApp.ReductionObjects
{
    public class RunningAverage<T> where T : INumber<T>
    {
        public readonly EventHandlerSync OnItemAdded = new();
        public T LastResult => _lastResult;
        private CircularMultiResolutionSum<T> _circularMultiResolutionArray;
        private CMRIndex _startIndex;
        private CMRIndex _endIndex;
        private T _itemCount;
        private T _lastResult = T.Zero;
        public RunningAverage(CircularMultiResolutionSum<T> src, uint to, uint from = 0)
        {
            if (to <= from) throw new ArgumentException("To needs to be bigger than from. ");
            _circularMultiResolutionArray = src;
            _startIndex = src.GetIndex(from);
            _endIndex = src.GetIndex(to);
            _itemCount = T.CreateTruncating(to - from);
            src.SubscribeValueAdded(Recalculate);
        }

        public void Recalculate()
        {
            T start = _circularMultiResolutionArray[_startIndex];
            T end = _circularMultiResolutionArray[_endIndex];
            _lastResult = (_circularMultiResolutionArray[_startIndex] - _circularMultiResolutionArray[_endIndex]) / _itemCount;
            OnItemAdded.Invoke();
        }
    }
}
