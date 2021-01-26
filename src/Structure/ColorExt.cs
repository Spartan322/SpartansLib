using System;
using Godot;

namespace SpartansLib.Structure
{
    [Serializable]
    public struct ColorExt : IEquatable<ColorExt>, IEquatable<Color>
    {
        public Color Color;

        public float r { get => Color.r; set => Color.r = value; }
        public float g { get => Color.g; set => Color.g = value; }
        public float b { get => Color.b; set => Color.b = value; }
        public float a { get => Color.a; set => Color.a = value; }

        public int r8 { get => Color.r8; set => Color.r8 = value; }
        public int g8 { get => Color.g8; set => Color.g8 = value; }
        public int b8 { get => Color.b8; set => Color.b8 = value; }
        public int a8 { get => Color.a8; set => Color.a8 = value; }

        public float h { get => Color.h; set => Color.h = value; }
        public float s { get => Color.s; set => Color.s = value; }
        public float v { get => Color.v; set => Color.v = value; }

        public float this[int index] { get => Color[index]; set => Color[index] = value; }

        public void ToHsv(out float hue, out float saturation, out float value)
            => Color.ToHsv(out hue, out saturation, out value);

        public Color Blend(Color over)
            => Color.Blend(over);

        public Color Contrasted()
            => Color.Contrasted();

        public Color Darkened(float amount)
            => Color.Darkened(amount);

        public Color Inverted()
            => Color.Inverted();

        public Color Lightened(float amount)
            => Color.Lightened(amount);

        public Color LinearInterpolate(Color c, float t)
            => Color.LinearInterpolate(c, t);

        public int ToAbgr32()
            => Color.ToAbgr32();

        public long ToAbgr64()
            => Color.ToAbgr64();

        public int ToArgb32()
            => Color.ToArgb32();

        public long ToArgb64()
            => Color.ToArgb64();

        public int ToRgba32()
            => Color.ToRgba32();

        public long ToRgba64()
            => Color.ToRgba64();

        public string ToHtml(bool includeAlpha = true)
            => Color.ToHtml(includeAlpha);

        public ColorExt(Color clr)
        {
            Color = clr;
        }

        public ColorExt(float r, float g, float b, float a = 1f)
        {
            Color = new Color(r, g, b, a);
        }

        public ColorExt(int rgba)
        {
            Color = new Color(rgba);
        }

        public ColorExt(long rgba)
        {
            Color = new Color(rgba);
        }

        private static int ParseCol8(string str, int ofs)
        {
            int num = 0;
            for (int i = 0; i < 2; i++)
            {
                int num2 = str[i + ofs];
                int num3;
                if (num2 >= 48 && num2 <= 57)
                {
                    num3 = num2 - 48;
                }
                else if (num2 >= 97 && num2 <= 102)
                {
                    num3 = num2 - 97;
                    num3 += 10;
                }
                else
                {
                    if (num2 < 65 || num2 > 70)
                    {
                        return -1;
                    }
                    num3 = num2 - 65;
                    num3 += 10;
                }
                num = ((i != 0) ? (num + num3) : (num + num3 * 16));
            }
            return num;
        }

        private string ToHex32(float val)
        {
            int num = Mathf.RoundToInt(Mathf.Clamp(val * 255f, 0f, 255f));
            string text = string.Empty;
            for (int i = 0; i < 2; i++)
            {
                int num2 = num & 0xF;
                char c = (num2 >= 10) ? ((char)(97 + num2 - 10)) : ((char)(48 + num2));
                num >>= 4;
                text = c.ToString() + text;
            }
            return text;
        }

        internal static bool HtmlIsValid(string color)
        {
            if (color.Length != 0)
            {
                if (color[0] == '#')
                {
                    color = color.Substring(1, color.Length - 1);
                }
                bool flag;
                switch (color.Length)
                {
                    case 8:
                        flag = true;
                        break;
                    case 6:
                        flag = false;
                        break;
                    default:
                        return false;
                }
                if (flag && ParseCol8(color, 0) < 0)
                {
                    return false;
                }
                int num = flag ? 2 : 0;
                if (ParseCol8(color, num) >= 0)
                {
                    if (ParseCol8(color, num + 2) >= 0)
                    {
                        if (ParseCol8(color, num + 4) >= 0)
                        {
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        public ColorExt(string rgba)
        {
            Color = new Color(rgba);
        }

        public static bool operator ==(ColorExt left, ColorExt right)
            => left.Equals(right.Color);

        public static bool operator !=(ColorExt left, ColorExt right)
            => !(left == right);

        public static bool operator <(ColorExt left, ColorExt right)
            => left.Color < right.Color;

        public static bool operator >(ColorExt left, ColorExt right)
            => left.Color > right.Color;

        public static implicit operator Color(ColorExt ext)
            => ext.Color;

        public static implicit operator ColorExt(Color clr)
            => new ColorExt(clr);

        public override bool Equals(object obj)
        {
            if (obj is ColorExt ext)
                return Equals(ext);
            if (obj is Color clr)
                return Equals(clr);
            return false;
        }

        public bool Equals(Color other)
            => r == other.r && g == other.g && b == other.b && a == other.a;

        public bool IsEqualApprox(Color other)
            =>Mathf.IsEqualApprox(r, other.r) && Mathf.IsEqualApprox(g, other.g) && Mathf.IsEqualApprox(b, other.b) && Mathf.IsEqualApprox(a, other.a);

        public override int GetHashCode()
            => r.GetHashCode() ^ g.GetHashCode() ^ b.GetHashCode() ^ a.GetHashCode();

        public override string ToString()
            => $"{r.ToString()},{g.ToString()},{b.ToString()},{a.ToString()}";

        public string ToString(string format)
            => $"{r.ToString(format)},{g.ToString(format)},{b.ToString(format)},{a.ToString(format)}";

        public bool Equals(ColorExt other)
            => Color.Equals(other.Color);

        public static ColorExt FromRGB8(int rgb, int alpha = 0xFF)
        {
            while (rgb > 0 && rgb <= 0xFFFFFF)
                rgb <<= 8;
            return new ColorExt((rgb & ~0xFF) | alpha);
        }

        public static ColorExt FromRGB8(int r, int g, int b, int alpha = 0xFF)
            => FromRGB8((byte)r, (byte)g, (byte)b, (byte)alpha);

        public static ColorExt FromRGBA8(uint rgba)
            => new ColorExt((int)rgba);

        public static ColorExt FromRGB16(long rgb, int alpha = 0xFFFF)
        {
            while (rgb > 0 && rgb <= 0xFFFFFFFFFFFF)
                rgb <<= 16;
            return new ColorExt((rgb & ~0xFFFF) | alpha);
        }

        public static ColorExt FromRGBA16(ulong rgba)
            => new ColorExt((long)rgba);

        public static ColorExt FromHSV(float h, float s, float v, float alpha = 1)
            => Color.FromHsv(h, s, v, alpha);

        public static ColorExt FromHSV8(int h, int s, int v, int alpha = 255)
            => Color.FromHsv(h/255f, s/255f, v/255f, alpha/255f);

        public static ColorExt FromString(string colorStr)
            => new ColorExt(colorStr);

        public static ColorExt FromRGB8(byte r, byte g, byte b, byte a = 255)
            => Color.Color8(r, g, b, a);

        public static ColorExt FromName(string name, int alpha = 255)
            => Color.ColorN(name, alpha / 255f);

        public static ColorExt FromName(string name, float alpha = 1)
            => Color.ColorN(name, alpha);
    }
}
