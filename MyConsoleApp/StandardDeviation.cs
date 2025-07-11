using System.Numerics;

namespace MyConsoleApp
{
    public class StandardDeviation<T> where T : INumber<T>, IRootFunctions<T>, IEquatable<T>, IComparable<T>
    {
        private readonly ICMRObject<T> _squaredSumSrc;
        private readonly ICMRObject<T> _sumSrc;
        private T _itemCount;
        private int _itemCountAsInt;
        private T _value = T.Zero;
        public T Value => _value;
        private bool _squareSumReceived;
        private bool _sumReceived;

        private CMRIndex _from;
        private CMRIndex _to;
        public StandardDeviation(ICMRObject<T> squaredSumSrc, ICMRObject<T> sumSrc, CMRIndex to, CMRIndex from, int itemCount)
        {
            if (squaredSumSrc.MaxSize != sumSrc.MaxSize)
                throw new ArgumentException("Sum and SquaredSum sources need to originate from the same array. ");
            if (itemCount <= 1)
                throw new ArgumentException("Standard deviation requires at least two items.", nameof(itemCount));

            _squaredSumSrc = squaredSumSrc;
            _sumSrc = sumSrc;
            _itemCount = T.CreateTruncating(itemCount);
            _itemCountAsInt = itemCount;
            _to = to;
            _from = from;
            squaredSumSrc.SubscribeValueAdded(ReceiveSquaredSum);
            sumSrc.SubscribeValueAdded(ReceiveSum);
        }

        public static (ICMRObject<T> squaredSumSrc, ICMRObject<T> sumSrc, CMRIndex to, CMRIndex from, int itemCount)
            CreateParameters<T>(CircularMultiResolutionArray<T> srcArray, uint to, uint from = 0) where T : INumber<T>, IRootFunctions<T>
        {
            CMRIndex fromIndex = srcArray.GetIndex(from);
            CMRIndex toIndex = srcArray.GetIndex(to);
            CMRIndex firstIndex = srcArray.GetIndex(0);
            SquaredValue<T> square = new SquaredValue<T>(srcArray, firstIndex);
            ICMRObject<T> squaredSumSrc = new CircularMultiResolutionSum<T>(square, srcArray.PartitionCount, srcArray.PartitionSize, srcArray.MagnitudeIncrease);
            ICMRObject<T> sumSrc = new CircularMultiResolutionSum<T>(srcArray, srcArray.PartitionCount, srcArray.PartitionSize, srcArray.MagnitudeIncrease);
            return (squaredSumSrc, sumSrc, toIndex, fromIndex, (int)(to - from));
        }


        private void ReceiveSquaredSum()
        {
            if (_squareSumReceived) Console.WriteLine("Received square sum twice in a standard deviation calculation. Check calculation tree. ");

            _squareSumReceived = true;

            if (_sumReceived)
            {
                Recalculate();
                _sumReceived = false; _squareSumReceived = false;
            }
        }

        private void ReceiveSum()
        {
            if (_sumReceived) Console.WriteLine("Received sum twice in a standard deviation calculation. Check calculation tree. ");

            _sumReceived = true;

            if (_squareSumReceived)
            {
                Recalculate();
                _sumReceived = false; _squareSumReceived = false;
            }
        }
        private void Recalculate()
        {
            if (_sumSrc.Count <= _itemCountAsInt) return;

            T sumOfSquares = _squaredSumSrc[_from] - _squaredSumSrc[_to];
            T sum = _sumSrc[_from] - _sumSrc[_to];

            T variance = (sumOfSquares - (sum * sum) / _itemCount) / (_itemCount - T.One);
            if (variance < T.Zero) variance = T.Zero;
            _value = T.Sqrt(variance);
        }
    }
}
