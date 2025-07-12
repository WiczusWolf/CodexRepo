using System.Numerics;

namespace MyConsoleApp
{
    public class QuadraticFit<T> where T : INumber<T>
    {
        private readonly ICMRObject<T> _sumSrc;
        private readonly ICMRObject<T> _xySrc;
        private readonly ICMRObject<T> _x2ySrc;
        private readonly CMRIndex _from;
        private readonly CMRIndex _to;
        private readonly int _itemCount;

        public T A { get; private set; } = T.Zero;
        public T B { get; private set; } = T.Zero;
        public T C { get; private set; } = T.Zero;

        public readonly EventHandlerSync OnCoefficientsUpdated = new();

        public QuadraticFit(ICMRObject<T> sumSrc, ICMRObject<T> xySrc, ICMRObject<T> x2ySrc, CMRIndex to, CMRIndex from, int itemCount)
        {
            if (sumSrc.MaxSize != xySrc.MaxSize || xySrc.MaxSize != x2ySrc.MaxSize)
                throw new ArgumentException("Sources must originate from the same array.");

            _sumSrc = sumSrc;
            _xySrc = xySrc;
            _x2ySrc = x2ySrc;
            _from = from;
            _to = to;
            _itemCount = itemCount;

            x2ySrc.SubscribeValueAdded(Recalculate);
        }

        public static (ICMRObject<T> sumSrc, ICMRObject<T> xySrc, ICMRObject<T> x2ySrc, CMRIndex to, CMRIndex from, int itemCount)
            CreateParameters(CircularMultiResolutionArray<T> srcArray, uint to, uint from = 0)
        {
            if (to <= from) throw new ArgumentException("To needs to be bigger than from.");

            var fromIndex = srcArray.GetIndex(from);
            var toIndex = srcArray.GetIndex(to);

            var sum = new CircularMultiResolutionSum<T>(srcArray, srcArray.PartitionCount, srcArray.PartitionSize, srcArray.MagnitudeIncrease);
            var xy = new CircularMultiResolutionWeightedSum<T>(srcArray, srcArray.PartitionCount, srcArray.PartitionSize, srcArray.MagnitudeIncrease);
            var x2y = new CircularMultiResolutionWeightedPowerSum<T>(srcArray, 2, srcArray.PartitionCount, srcArray.PartitionSize, srcArray.MagnitudeIncrease);

            return (sum, xy, x2y, toIndex, fromIndex, (int)(to - from));
        }

        private static double Sum1(long m) => m * (m + 1) / 2.0;
        private static double Sum2(long m) => m * (m + 1) * (2 * m + 1) / 6.0;
        private static double Sum3(long m)
        {
            double t = m * (m + 1) / 2.0;
            return t * t;
        }
        private static double Sum4(long m) => m * (m + 1) * (2 * m + 1) * (3 * m * m + 3 * m - 1) / 30.0;

        private void Recalculate()
        {
            if (_sumSrc.Count <= _itemCount) return;

            T sy = _sumSrc[_from] - _sumSrc[_to];
            T sxy = _xySrc[_from] - _xySrc[_to];
            T sx2y = _x2ySrc[_from] - _x2ySrc[_to];

            double Sy = double.CreateTruncating(sy);
            double Sxy = double.CreateTruncating(sxy);
            double Sx2y = double.CreateTruncating(sx2y);

            double N = _itemCount;
            long start = _sumSrc.Count - _itemCount;
            long end = start + _itemCount - 1;
            double SxVal = Sum1(end) - Sum1(start - 1);
            double Sx2Val = Sum2(end) - Sum2(start - 1);
            double Sx3Val = Sum3(end) - Sum3(start - 1);
            double Sx4Val = Sum4(end) - Sum4(start - 1);

            // Gaussian elimination
            double a1 = Sx4Val, b1 = Sx3Val, c1 = Sx2Val, d1 = Sx2y;
            double a2 = Sx3Val, b2 = Sx2Val, c2 = SxVal, d2 = Sxy;
            double a3 = Sx2Val, b3 = SxVal, c3 = N, d3 = Sy;

            double f21 = a2 / a1; a2 = 0; b2 -= f21 * b1; c2 -= f21 * c1; d2 -= f21 * d1;
            double f31 = a3 / a1; a3 = 0; b3 -= f31 * b1; c3 -= f31 * c1; d3 -= f31 * d1;
            double f32 = b3 / b2; b3 = 0; c3 -= f32 * c2; d3 -= f32 * d2;

            double c = d3 / c3;
            double b = (d2 - c2 * c) / b2;
            double a = (d1 - b1 * b - c1 * c) / a1;

            A = T.CreateTruncating(a);
            B = T.CreateTruncating(b);
            C = T.CreateTruncating(c);

            OnCoefficientsUpdated.Invoke();
        }
    }
}
