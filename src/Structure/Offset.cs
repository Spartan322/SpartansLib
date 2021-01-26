using System;
using Godot;
using SpartansLib.Structure;

namespace SpartansLib.Structure
{
    public struct Offset
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.x;
                Y = value.y;
            }
        }

        public Offset(float x = 0, float y = 0, float rotation = 0)
        {
            X = x;
            Y = y;
            Rotation = rotation;
        }

        public Offset(Vector2 position, float rotation = 0) : this(rotation: rotation)
        {
            Position = position;
        }

        public Offset(Vector2 position, Vector2 aimAt) : this(position, position.AngleToPoint(aimAt)) { }

        public void Deconstruct(out Vector2 position, out Angle rotation)
        {
            position = Position;
            rotation = Rotation;
        }

        public void Deconstruct(out float x, out float y, out Angle rotation)
        {
            x = X;
            y = Y;
            rotation = Rotation;
        }

        public static implicit operator Offset(ValueTuple<Vector2, Angle> t)
            => new Offset(t.Item1, t.Item2);

        public static implicit operator Offset(ValueTuple<float, float, Angle> t)
            => new Offset(t.Item1, t.Item2, t.Item3);
    }
}
