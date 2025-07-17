using System.Runtime.CompilerServices;
namespace MyConsoleApp
{
    public static class IntMath
    {

        public static int QuadraticOffset(int index, int magnitude) =>
            index < magnitude >> 1
            ? -(magnitude - (index << 1) - 1)
            : (index << 1) - magnitude + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(this int n) => n > 0 && (n & n - 1) == 0;

        /// <summary>
        /// Works only for mod = 2^x
        /// </summary>
        /// <returns>A positive integer for any positive mod.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastAbsMod(int n, int mod)
        {
            return n & (mod - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastMax(int a, int b)
        {
            int diff = a - b;
            int dsgn = diff >> 31;
            return a - (diff & dsgn);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastMin(int a, int b)
        {
            int diff = a - b;
            int dsgn = diff >> 31;
            return b + (diff & dsgn);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastAbs(int x)
        {
            int mask = x >> 31;         // -1 if x < 0, 0 if x ≥ 0
            return x + mask ^ mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivZeroForRange(int value, int min, int max)
        {
            return value / max + (value >> 31);
        }
        public static int Pow(int item, int pow)
        {
            int magnitude = 1;
            for (int i = 0; i < pow; i++)
            {
                magnitude *= item;
            }
            return magnitude;
        }

    }
}
