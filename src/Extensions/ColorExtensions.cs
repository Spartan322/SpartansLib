using System;
using Godot;

namespace SpartansLib.Extensions
{
    public static class ColorExtensions
    {
        public static Color R(this Color color, float value)
            => new Color(value, color.g, color.b, color.a);

        public static Color G(this Color color, float value)
            => new Color(color.r, value, color.b, color.a);

        public static Color B(this Color color, float value)
            => new Color(color.r, color.g, value, color.a);

        public static Color A(this Color color, float value)
            => new Color(color.r, color.g, color.b, value);

        public static Color R8(this Color color, int value)
            => new Color((value & 0xFF) / 255f, color.g, color.b, color.a);

        public static Color G8(this Color color, int value)
            => new Color(color.r, (value & 0xFF) / 255f, color.b, color.a);

        public static Color B8(this Color color, int value)
            => new Color(color.r, color.g, (value & 0xFF) / 255f, color.a);

        public static Color A8(this Color color, int value)
            => new Color(color.r, color.g, color.b, (value & 0xFF) / 255f);

        public static float R(this Color color)
            => color.r;

        public static float G(this Color color)
            => color.g;

        public static float B(this Color color)
            => color.b;

        public static float A(this Color color)
            => color.a;

        public static int R8(this Color color)
            => color.r8;

        public static int G8(this Color color)
            => color.g8;

        public static int B8(this Color color)
            => color.b8;

        public static int A8(this Color color)
            => color.a8;
    }
}
