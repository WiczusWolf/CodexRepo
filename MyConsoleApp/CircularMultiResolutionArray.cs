using Common.Delegates;
using System.Numerics;

namespace MyConsoleApp
{
	public class CircularMultiResolutionArray<T> where T : INumber<T>
	{
		private readonly int _partitions;
		private readonly int _size;
		private readonly int _increase;
		private readonly T[][] _arrays;
		private readonly int[] _starts;
		private readonly T[] _sums;
		private readonly int[] _counts;
		private int _filledCount;

		public readonly EventHandlerSync<T> OnItemAdded = new();
		public readonly EventHandlerSync<T> OnItemRemoved = new();

		public int Partitions => _partitions;
		public int Size => _size;
		public int Increase => _increase;

		public CircularMultiResolutionArray(int partitions, int size, int increase)
		{
			if (partitions <= 0) throw new ArgumentOutOfRangeException(nameof(partitions));
			if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (increase <= 0) throw new ArgumentOutOfRangeException(nameof(increase));

			_partitions = partitions;
			_size = size;
			_increase = increase;
			_arrays = new T[partitions][];
			_starts = new int[partitions];
			_sums = new T[partitions];
			_counts = new int[partitions];
			_filledCount = 0;

			for (int i = 0; i < partitions; i++)
			{
				_arrays[i] = new T[size];
				_starts[i] = 0;
				_sums[i] = T.Zero;
				_counts[i] = 0;
			}
		}

		public void PushFront(T item)
		{
			int start = (_starts[0] - 1 + _size) % _size;
			bool removed = false;
			T removedValue = _arrays[0][start];
			if (_filledCount == _size)
			{
				removed = true;
			}
			else
			{
				_filledCount++;
			}

			_starts[0] = start;
			_arrays[0][start] = item;

			_sums[0] += item;
			_counts[0]++;

			if (_counts[0] >= _increase)
			{
				T avg = _sums[0] / T.CreateChecked(_increase);
				_sums[0] = T.Zero;
				_counts[0] = 0;
				PushToLevel(1, avg);
			}

			OnItemAdded?.Invoke(item);
			if (removed)
			{
				OnItemRemoved?.Invoke(removedValue);
			}
		}

		private void PushToLevel(int level, T item)
		{
			if (level >= _partitions)
				return;

			int start = (_starts[level] - 1 + _size) % _size;
			_starts[level] = start;
			_arrays[level][start] = item;

			_sums[level] += item;
			_counts[level]++;

			if (_counts[level] >= _increase)
			{
				T avg = _sums[level] / T.CreateChecked(_increase);
				_sums[level] = T.Zero;
				_counts[level] = 0;
				PushToLevel(level + 1, avg);
			}
		}

		public T this[int partition, int index]
		{
			get
			{
				if (partition < 0 || partition >= _partitions)
					throw new ArgumentOutOfRangeException(nameof(partition));
				if (index < 0 || index >= _size)
					throw new ArgumentOutOfRangeException(nameof(index));
				int realIndex = (_starts[partition] + index) % _size;
				return _arrays[partition][realIndex];
			}
		}

		public int GetStartIndex(int partition)
		{
			if (partition < 0 || partition >= _partitions)
				throw new ArgumentOutOfRangeException(nameof(partition));
			return _starts[partition];
		}
	}
}
