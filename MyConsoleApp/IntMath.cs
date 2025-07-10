using System.Runtime.CompilerServices;

namespace MyConsoleApp
{
    public static class IntMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(this int n) => n > 0 && (n & n - 1) == 0;
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
