using System.Collections.Generic;
using System;
using System.Collections;

namespace SpartansLib.Structure
{
    public class Grid2D<T> : IEnumerable<T>
    {
        // TODO implement enumerable direction flags
        [Flags]
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        private int _width, _height;
        private T[] _gridArray;
        public readonly Direction GridDirection;

        public Grid2D(int width, int height, Direction direction = Direction.Horizontal)
        {
            _width = width;
            _height = height;
            _gridArray = new T[width*height];
            GridDirection = direction;
        }

        public Grid2D(int width, int height, IEnumerable<T> fill, Direction direction = Direction.Horizontal)
            : this(width, height, direction)
        {
            CopyFrom(fill);
        }

        public int Width => _width;
        public int Height => _height;

        public T this[int x, int y]
        {
            get => TryGetAt(x, y, out var result)
                ? result
                : throw new IndexOutOfRangeException();
            set
            {
                if(!TrySetAt(x, y, value))
                    throw new IndexOutOfRangeException();
            }
        }

        public T this[Vec2i position]
        {
            get => this[position.X, position.Y];
            set => this[position.X, position.Y] = value;
        }

        public bool TryGetAt(int x, int y, out T result)
        {
            if(y < 0 || y > _height || x < 0 || x > _width)
            {
                result = default;
                return false;
            }
            result = _gridArray[_CalculatePosition(x, y)];
            return true;
        }

        public bool TrySetAt(int x, int y, T value)
        {
            if(y < 0 || y > _height || x < 0 || x > _width)
                return false;
            _gridArray[_CalculatePosition(x, y)] = value;
            return true;
        }

        public void CopyFrom(IEnumerable<T> copy)
        {
            var copyEnum = copy.GetEnumerator();
            for(var i = 0; i < _gridArray.Length; i++)
            {
                if(!copyEnum.MoveNext()) break;
                _gridArray[i] = copyEnum.Current;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(var value in _gridArray)
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator() => _gridArray.GetEnumerator();

        private int _CalculatePosition(int x, int y) 
        {
            if(GridDirection == Direction.Horizontal) return x+Width*y;
            return y+Height*x;
        }
    }
}