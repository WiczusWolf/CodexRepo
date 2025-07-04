namespace Common.Delegates
{
	public class EventHandlerSync<TArg, TRes>
	{
		private Func<TArg, TRes>[] _eventHandlerSync;
		private int _count;

		public EventHandlerSync(int capacity = 4)
		{
			_eventHandlerSync = new Func<TArg, TRes>[capacity];
			_count = 0;
		}

		public int Count => _count;

		public void Add(Func<TArg, TRes> eventHandlerSync)
		{
			if (_count == _eventHandlerSync.Length)
				Expand();

			_eventHandlerSync[_count++] = eventHandlerSync;
		}

		public void Remove(Func<TArg, TRes> eventHandlerSync)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_eventHandlerSync[i] == eventHandlerSync)
				{
					_count--;
					_eventHandlerSync[i] = _eventHandlerSync[_count];
					_eventHandlerSync[_count] = null!;
					return;
				}
			}
		}

		public TRes[] Invoke(TArg arg)
		{
			var results = new TRes[_count];
			for (int i = 0; i < _count; i++)
			{
				results[i] = _eventHandlerSync[i](arg);
			}
			return results;
		}

		private void Expand()
		{
			int newSize = _eventHandlerSync.Length * 2;
			var newArray = new Func<TArg, TRes>[newSize];
			Array.Copy(_eventHandlerSync, newArray, _eventHandlerSync.Length);
			_eventHandlerSync = newArray;
		}

		public static EventHandlerSync<TArg, TRes> operator +(EventHandlerSync<TArg, TRes> eventHandlerSync, Func<TArg, TRes> action)
		{
			eventHandlerSync.Add(action);
			return eventHandlerSync;
		}

		public static EventHandlerSync<TArg, TRes> operator -(EventHandlerSync<TArg, TRes> eventHandlerSync, Func<TArg, TRes> action)
		{
			eventHandlerSync.Remove(action);
			return eventHandlerSync;
		}
	}

	public class EventHandlerSync
	{
		private Action[] _eventHandlerSyncs;
		private int _count;

		public EventHandlerSync(int capacity = 4)
		{
			_eventHandlerSyncs = new Action[capacity];
			_count = 0;
		}

		public int Count => _count;

		public void Add(Action eventHandlerSync)
		{
			if (_count == _eventHandlerSyncs.Length)
				Expand();

			_eventHandlerSyncs[_count++] = eventHandlerSync;
		}

		public void Remove(Action eventHandlerSync)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_eventHandlerSyncs[i] == eventHandlerSync)
				{
					_count--;
					_eventHandlerSyncs[i] = _eventHandlerSyncs[_count];
					_eventHandlerSyncs[_count] = null!;
					return;
				}
			}
		}

		public void Invoke()
		{
			for (int i = 0; i < _count; i++)
			{
				_eventHandlerSyncs[i]();
			}
		}

		private void Expand()
		{
			int newSize = _eventHandlerSyncs.Length * 2;
			var newArray = new Action[newSize];
			Array.Copy(_eventHandlerSyncs, newArray, _eventHandlerSyncs.Length);
			_eventHandlerSyncs = newArray;
		}

		public static EventHandlerSync operator +(EventHandlerSync eventHandlerSync, Action action)
		{
			eventHandlerSync.Add(action);
			return eventHandlerSync;
		}

		public static EventHandlerSync operator -(EventHandlerSync eventHandlerSync, Action action)
		{
			eventHandlerSync.Remove(action);
			return eventHandlerSync;
		}
	}

	public class EventHandlerSync<TArg>
	{
		private Action<TArg>[] _eventHandlerSyncs;
		private int _count;

		public EventHandlerSync(int capacity = 4)
		{
			_eventHandlerSyncs = new Action<TArg>[capacity];
			_count = 0;
		}

		public int Count => _count;

		public void Add(Action<TArg> eventHandlerSync)
		{
			if (_count == _eventHandlerSyncs.Length)
				Expand();

			_eventHandlerSyncs[_count++] = eventHandlerSync;
		}

		public void Remove(Action<TArg> eventHandlerSync)
		{
			for (int i = 0; i < _count; i++)
			{
				if (_eventHandlerSyncs[i] == eventHandlerSync)
				{
					_count--;
					_eventHandlerSyncs[i] = _eventHandlerSyncs[_count]; // move last to fill gap
					_eventHandlerSyncs[_count] = null!; // Changes invocation order
					return;
				}
			}
		}

		public void Invoke(TArg value)
		{
			for (int i = 0; i < _count; i++)
			{
				_eventHandlerSyncs[i](value);
			}
		}

		private void Expand()
		{
			int newSize = _eventHandlerSyncs.Length * 2;
			var newArray = new Action<TArg>[newSize];
			Array.Copy(_eventHandlerSyncs, newArray, _eventHandlerSyncs.Length);
			_eventHandlerSyncs = newArray;
		}

		public static EventHandlerSync<TArg> operator +(EventHandlerSync<TArg> eventHandlerSync, Action<TArg> action)
		{
			eventHandlerSync.Add(action);
			return eventHandlerSync;
		}

		public static EventHandlerSync<TArg> operator -(EventHandlerSync<TArg> eventHandlerSync, Action<TArg> action)
		{
			eventHandlerSync.Remove(action);
			return eventHandlerSync;
		}
	}
}
