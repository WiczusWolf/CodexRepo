using System.Numerics;
using static MyConsoleApp.IntMath;
namespace MyConsoleApp
{
    public static class INumberMath
    {
        public static T SwitchOnGreaterOrEqualZero<T>(int comparator, T onLower, T onGreater) where T : INumber<T>
        {
            int isGE = ~(comparator >> 31) & 1;
            T flag = T.CreateTruncating(isGE);
            return onLower + (onGreater - onLower) * flag;
        }

        //Triangle interpolation that has local extrema at current. If offset = maxOffset the value will be the next value. Respectively -offset = maxoffset will be the previous. 
        public static T Interpolate<T>(T current, T previous, T next, int offset, int maxOffset) where T : INumber<T>
        {
            int absOff = FastAbs(offset);
            var invMax = T.One / T.CreateTruncating(maxOffset);

            T frac = T.CreateTruncating(absOff) * invMax;

            T currFrac = T.One - frac;
            return current * currFrac + SwitchOnGreaterOrEqualZero(offset, frac * previous, frac * next);
        }
        public static T Interpolate<T>(T current, T next, int offset, int maxOffset) where T : INumber<T>
        {
            var frac = T.CreateTruncating(offset) / T.CreateTruncating(maxOffset);
            return current + (next - current) * frac;
        }
    }
}
