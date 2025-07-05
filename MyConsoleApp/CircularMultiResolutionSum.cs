using System.Numerics;

namespace MyConsoleApp
{
    public class CircularMultiResolutionSum<T> where T : INumber<T>
    {

        public CircularMultiResolutionSum(CircularMultiResolutionArray<T> src)
        {

        }


        public T this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
