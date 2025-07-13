using System.Numerics;

namespace MyConsoleApp
{
    public class SquaredValue<T> : ICMRObject<T> where T : INumber<T>
    {
        public readonly EventHandlerSync OnItemAdded = new();

        private readonly ICMRObject<T> _src;
        private readonly CMRIndex _at;
        private T _value = T.Zero;
        public T Value => _value;

        public int Count => 1;

        public int MaxSize => 1;

        public T this[CMRIndex index] => Value;

        public SquaredValue(ICMRObject<T> src, CMRIndex at)
        {
            _src = src;
            _at = at;
            src.SubscribeValueAdded(Recalculate);
        }

        public void Recalculate()
        {
            T value = _src[_at];
            _value = value * value;
        }

        public void SubscribeValueAdded(Action action) => _src.SubscribeValueAdded(action);
        public void UnsubscribeValueAdded(Action action) => _src.UnsubscribeValueAdded(action);

        public T First()
        {
            return _value;
        }
    }
}
