namespace MyConsoleApp
{

    public interface ICMRObject<T> : IItemAddedObservable, ICMRIndexAccessible<T>
    {

    }
    public interface IItemAddedObservable
    {
        void SubscribeValueAdded(Action action);
        void UnsubscribeValueAdded(Action action);
    }

    public interface ICMRIndexAccessible<T>
    {
        T this[CMRIndex index] { get; }
        T First();
        int Count { get; }
        int MaxSize { get; }
    }
}
