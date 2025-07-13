using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyConsoleApp.CMRObject;
using System.Numerics;
using static MyConsoleApp.IntMath;
using static MyConsoleApp.INumberMath;

namespace MyConsoleApp.Tests.CMRObjectTests
{
    [TestClass]
    public class CircularMultiResolutionBaseTests
    {
        private class CircularMultiResolutionBaseImpl<T> : CircularMultiResolutionBase<T> where T : INumber<T>
        {
            protected readonly T[][] _partitions;
            protected readonly T[] _removed;
            public CircularMultiResolutionBaseImpl(int partitionCount, int partitionSize, int magnitudeIncrease) : base(partitionCount, partitionSize, magnitudeIncrease)
            {
                _partitions = new T[partitionCount][];
                _removed = new T[partitionCount];
                for (int i = 0; i < _partitionCount; i++)
                {
                    _partitions[i] = new T[partitionSize];
                }
            }

            public override T this[CMRIndex index]
            {
                get
                {
                    int partitionIndex = index.PartitionIndex;
                    int itemIndex = index.ItemIndex;
                    int itemOffset = index.Offset;

                    T current = GetWithNonCircularItemIndex(_partitions, partitionIndex, itemIndex);
                    T next = SwitchOnGreaterOrEqualZero(itemIndex - _partitionSize + 1,
                        GetWithNonCircularItemIndex(_partitions, partitionIndex, FastMin(_partitionSize - 1, itemIndex + 1)),
                        _removed[partitionIndex]);
                    T previous = GetWithNonCircularItemIndex(_partitions, partitionIndex, itemIndex - 1);

                    var (offset, maxOffset) = ComputeOffsetFromPartitionEnd(partitionIndex, itemOffset);
                    return Interpolate(current, previous, next, offset, maxOffset);
                }
            }

            public override T First() => T.Zero;

            protected override void Assign(int realPartitionIndex, int realItemIndex)
            {
                _removed[realPartitionIndex] = _partitions[realPartitionIndex][realItemIndex];
                _partitions[realPartitionIndex][realItemIndex] = GetWithNonCircularItemIndex(_partitions, 0, 0);
            }

            protected override void AssignFirst(T value, int realItemIndex)
            {
                _removed[0] = _partitions[0][realItemIndex];
                _partitions[0][realItemIndex] = value;
            }

            protected override void PostItemPush() { }
        }

        [TestMethod]
        public void Constructor_Throws_OnNonPowerOfTwoPartitionSize()
        {
            Assert.ThrowsException<ArgumentException>(() =>
                new CircularMultiResolutionBaseImpl<float>(2, 3, 2));
        }

        [TestMethod]
        public void Constructor_Throws_OnInvalidMagnitudeIncrease()
        {
            Assert.ThrowsException<ArgumentException>(() =>
                new CircularMultiResolutionBaseImpl<float>(2, 4, 3));
        }

        [TestMethod]
        public void Constructor_Throws_WhenMagnitudeIncreaseTooLarge()
        {
            Assert.ThrowsException<ArgumentException>(() =>
                new CircularMultiResolutionBaseImpl<float>(2, 4, 8));
        }

        [TestMethod]
        public void GetIndex_Throws_OnIndexTooLarge()
        {
            var arr = new CircularMultiResolutionBaseImpl<float>(2, 4, 2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => arr.GetIndex((arr.MaxSize + 1)));
        }

        [TestMethod]
        public void SimplePush()
        {
            var arr = new CircularMultiResolutionBaseImpl<float>(1, 4, 2);

            for (int i = 1; i < 5; i++)
            {
                arr.PushFront(i);
                Assert.AreEqual(arr.Count, i);
                for (int j = 0; j < i; j++)
                {
                    Assert.AreEqual(i - j, arr[arr.GetIndex(j)]);
                }
            }
        }

        [TestMethod]
        public void PushPastMax()
        {
            var arr = new CircularMultiResolutionBaseImpl<float>(1, 4, 2);

            for (int i = 0; i < 5; i++)
            {
                arr.PushFront(i);
            }
            Assert.AreEqual(arr.Count, 4);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(4 - i, arr[arr.GetIndex(i)]);
            }
        }

        [TestMethod]
        public void PushIntoSecondPartition()
        {
            var arr = new CircularMultiResolutionBaseImpl<float>(2, 4, 2);
            Assert.AreEqual(8, arr.MaxSize);

            for (int i = 0; i < 15; i++)
            {
                arr.PushFront(i);
                for (int j = 0; j <= Math.Min(i, 7); j++)
                {
                    Assert.AreEqual(i - j, arr[arr.GetIndex(j)]);
                }
            }
        }
    }
}
