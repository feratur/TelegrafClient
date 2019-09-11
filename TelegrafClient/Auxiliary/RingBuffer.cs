namespace TelegrafClient.Auxiliary
{
    public class RingBuffer<T>
    {
        private readonly T[] _buffer;

        private int _startPtr;
        private int _count;

        public RingBuffer(int size) => _buffer = new T[size];

        public T Get()
        {
            if (IsEmpty)
                return default(T);

            var result = _buffer[_startPtr];

            --_count;

            AdvancePosition();

            return result;
        }

        public void Put(T item)
        {
            _buffer[PositionFromStart(_count)] = item;

            if (IsFull)
                AdvancePosition();
            else
                ++_count;
        }

        public bool TryInsertFirst(T item)
        {
            if (IsFull)
                return false;

            AdvancePosition(_buffer.Length - 1);

            ++_count;

            _buffer[_startPtr] = item;

            return true;
        }

        public bool IsFull => _count == _buffer.Length;

        public bool IsEmpty => _count == 0;

        private void AdvancePosition(int shift = 1) => _startPtr = PositionFromStart(shift);

        private int PositionFromStart(int i) =>
            (_startPtr + i) % _buffer.Length;
    }
}
