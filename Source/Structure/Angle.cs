using System;
using Godot;

namespace SpartansLib.Structure
{
    public struct Angle : 
        IEquatable<Angle>,
        IEquatable<float>,
        IComparable,
        IComparable<Angle>,
        IComparable<float>,
        IConvertible,
        IFormattable
    {
        private float radians;

        public float Radians
        {
            get => radians;
            set => radians = NormalizeToFloat(value, true);
        }
        public float Degrees
        {
            get => Mathf.Rad2Deg(Radians);
            set => Radians = Mathf.Deg2Rad(value);
        }

        public Angle(float ang, bool rad = false)
        {
            radians = 0;
            if (rad) Radians = ang;
            else Degrees = ang;
        }

        public static Angle Normalize(float angle, bool positiveRange = false)
            => NormalizeToFloat(angle, positiveRange);

        public static float NormalizeToFloat(float angle, bool positiveRange = false)
            => positiveRange ? Utility.Normalize(angle, 0, Mathf.Tau) : 2 * angle % Mathf.Tau - angle;

        public Angle Normalize(bool positiveRange = false)
            => NormalizeToFloat(Radians, positiveRange);

        public Angle Clamp(Angle min, Angle max)
            => Mathf.Clamp(Radians, min.Radians, max.Radians);

        public Angle Lerp(Angle end, float weight)
            => Mathf.Lerp(Radians, end.Radians, weight);

        public Angle LerpAngle(Angle end, float weight)
            => Mathf.LerpAngle(this, end, weight);

        public float Sin() => Mathf.Sin(Radians);
        public float Sinh() => Mathf.Sinh(Radians);
        public float Asin() => Mathf.Asin(Radians);
        public float Cos() => Mathf.Cos(Radians);
        public float Cosh() => Mathf.Cosh(Radians);
        public float Acos() => Mathf.Acos(Radians);
        public float Tan() => Mathf.Tan(Radians);
        public float Tanh() => Mathf.Tanh(Radians);
        public float Atan() => Mathf.Atan(Radians);
        public Angle Abs() => Mathf.Abs(Radians);
        public Vector2 ForwardVector() => new Vector2(Cos(), Sin());

        public static Angle Atan2(float x, float y) => Mathf.Atan2(x, y);

        public bool Equals(Angle other) => Equals(other.Radians);
        public bool Equals(float other) => Mathf.IsEqualApprox(Radians, other);

        public override bool Equals(object obj)
        {
            if (obj is Angle a)
                return Equals(a);
            if (obj is float f)
                return Equals(f);
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode() => Radians.GetHashCode();
        public override string ToString() => Radians.ToString();

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj is Angle a) return CompareTo(a);
            if (obj is float f) return CompareTo(f);
            if (Equals(obj)) return 0;
            return 0f.CompareTo(obj);
        }

        public int CompareTo(float other) => Radians.CompareTo(other);
        public int CompareTo(Angle other) => Radians.CompareTo(other.Radians);

        public TypeCode GetTypeCode() => TypeCode.Single;

        public bool ToBoolean(IFormatProvider provider)
            => Convert.ToBoolean(Radians, provider);

        public byte ToByte(IFormatProvider provider)
            => Convert.ToByte(Radians, provider);

        public char ToChar(IFormatProvider provider)
            => Convert.ToChar(Radians, provider);

        public DateTime ToDateTime(IFormatProvider provider)
            => Convert.ToDateTime(Radians, provider);

        public decimal ToDecimal(IFormatProvider provider)
            => Convert.ToDecimal(Radians, provider);

        public double ToDouble(IFormatProvider provider)
            => Convert.ToDouble(Radians, provider);

        public short ToInt16(IFormatProvider provider)
            => Convert.ToInt16(Radians, provider);

        public int ToInt32(IFormatProvider provider)
            => Convert.ToInt32(Radians, provider);

        public long ToInt64(IFormatProvider provider)
            => Convert.ToInt64(Radians, provider);

        public sbyte ToSByte(IFormatProvider provider)
            => Convert.ToSByte(Radians, provider);

        public float ToSingle(IFormatProvider provider)
            => Convert.ToSingle(Radians, provider);

        public string ToString(IFormatProvider provider)
            => Radians.ToString(provider);

        public object ToType(Type conversionType, IFormatProvider provider)
            => Convert.ChangeType(Radians, conversionType, provider);

        public ushort ToUInt16(IFormatProvider provider)
            => Convert.ToUInt16(Radians, provider);

        public uint ToUInt32(IFormatProvider provider)
            => Convert.ToUInt32(Radians, provider);

        public ulong ToUInt64(IFormatProvider provider)
            => Convert.ToUInt64(Radians, provider);

        public string ToString(string format, IFormatProvider formatProvider)
            => Radians.ToString(format, formatProvider);

        public static Angle operator +(Angle l, Angle r)
            => new Angle(l.Radians + r.Radians).Normalize(true);
        public static Angle operator +(Angle l, float r)
            => new Angle(l.Radians + r).Normalize(true);
        public static Angle operator +(float l, Angle r)
            => new Angle(l + r.Radians).Normalize(true);

        public static Angle operator -(Angle l, Angle r)
            => new Angle(l.Radians - r.Radians).Normalize(true);
        public static Angle operator -(Angle l, float r)
            => new Angle(l.Radians - r).Normalize(true);
        public static Angle operator -(float l, Angle r)
            => new Angle(l - r.Radians).Normalize(true);

        public static Angle operator /(Angle l, Angle r)
            => new Angle(l.Radians / r.Radians).Normalize(true);
        public static Angle operator /(Angle l, float r)
            => new Angle(l.Radians / r).Normalize(true);
        public static Angle operator /(float l, Angle r)
            => new Angle(l / r.Radians).Normalize(true);

        public static Angle operator *(Angle l, Angle r)
            => new Angle(l.Radians * r.Radians).Normalize(true);
        public static Angle operator *(Angle l, float r)
            => new Angle(l.Radians * r).Normalize(true);
        public static Angle operator *(float l, Angle r)
            => new Angle(l * r.Radians).Normalize(true);

        public static bool operator ==(Angle l, Angle r)
            => l.Equals(r);
        public static bool operator ==(Angle l, float r)
            => l.Equals(r);
        public static bool operator ==(float l, Angle r)
            => r.Equals(l);

        public static bool operator !=(Angle l, Angle r)
            => !(l == r);
        public static bool operator !=(Angle l, float r)
            => !(l == r);
        public static bool operator !=(float l, Angle r)
            => !(l == r);

        public static implicit operator Angle(float value)
            => new Angle(value, true);

        public static implicit operator Angle(double value)
            => new Angle((float)value, true);

        public static implicit operator float(Angle value)
            => value.Normalize(true).Radians;

        public static implicit operator double(Angle value)
            => (float)value;
    }
}
