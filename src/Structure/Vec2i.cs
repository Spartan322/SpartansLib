using System;
using Godot;
using SpartansLib.Extensions;
using SpartansLib.Structure;

namespace SpartansLib.Structure
{
    public readonly struct Vec2i
    {
        public int X { get; }
        public int Y { get; }

        public Vec2i(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public Vec2i(Vector2 position)
        {
            X = (int)position.x;
            Y = (int)position.y;
        }

        public void Deconstruct(out float x, out float y)
        {
            x = X;
            y = Y;
        }

        public static implicit operator Vec2i(Vector2 t)
            => new Vec2i(t);
    }
}
