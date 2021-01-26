using System;
using Godot;
using SpartansLib.Structure;

namespace SpartansLib.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 X(this Vector2 self, float x)
            => new Vector2(x, self.y);

        public static Vector2 Y(this Vector2 self, float y)
            => new Vector2(self.x, y);

        public static Vector2 X(this Vector2 self, Vector2 xSource)
            => new Vector2(self.y, xSource.x);

        public static Vector2 Y(this Vector2 self, Vector2 ySource)
            => new Vector2(ySource.y, self.x);

        public static float X(this Vector2 self)
            => self.x;

        public static float Y(this Vector2 self)
            => self.y;

        public static Vector2 Add(this Vector2 self, int v)
            => new Vector2(self.x + v, self.y + v);

        public static Vector2 Add(this Vector2 self, float v)
            => new Vector2(self.x + v, self.y + v);

        public static Vector2 Sub(this Vector2 self, int v)
            => new Vector2(self.x - v, self.y - v);

        public static Vector2 Sub(this Vector2 self, float v)
            => new Vector2(self.x - v, self.y - v);

        public static Vector2 AddX(this Vector2 self, int v)
            => new Vector2(self.x + v, self.y);

        public static Vector2 AddX(this Vector2 self, float v)
            => new Vector2(self.x + v, self.y);

        public static Vector2 SubX(this Vector2 self, int v)
            => new Vector2(self.x - v, self.y);

        public static Vector2 SubX(this Vector2 self, float v)
            => new Vector2(self.x - v, self.y);

        public static Vector2 AddY(this Vector2 self, int v)
            => new Vector2(self.x, self.y + v);

        public static Vector2 AddY(this Vector2 self, float v)
            => new Vector2(self.x, self.y + v);

        public static Vector2 SubY(this Vector2 self, int v)
            => new Vector2(self.x, self.y - v);

        public static Vector2 SubY(this Vector2 self, float v)
            => new Vector2(self.x, self.y - v);

        public static Vector2 MakeRotation(this Vector2 self, Angle theta)
            => theta.ForwardVector() * self.Length();

        public static Angle Atan2(this Vector2 self)
            => Angle.Atan2(self.x, self.y);

        public static Vector2 Max(this Vector2 self, Vector2 other)
            => new Vector2(Mathf.Max(self.x, other.x), Mathf.Max(self.y, other.y));

        public static Vector2 Min(this Vector2 self, Vector2 other)
            => new Vector2(Mathf.Min(self.x, other.x), Mathf.Min(self.y, other.y));

        public static Vector2 Vec2(this int self)
            => new Vector2(self, self);

        public static Vector2 Vec2(this float self)
            => new Vector2(self, self);
    }
}
