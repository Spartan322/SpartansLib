using System;
using Godot;
using SpartansLib.Structure;

namespace SpartansLib.Extensions
{
    public static class FloatExtensions
    {
        public static float Cos(this float self, bool degrees = false)
            => Mathf.Cos(degrees ? Mathf.Deg2Rad(self) : self);
        public static float Sin(this float self, bool degrees = false)
            => Mathf.Sin(degrees ? Mathf.Deg2Rad(self) : self);
        public static float Tan(this float self, bool degrees = false)
            => Mathf.Tan(degrees ? Mathf.Deg2Rad(self) : self);
        public static float NormalizeAngle(this float self, bool degrees = false)
            => Angle.NormalizeToFloat(degrees ? Mathf.Deg2Rad(self) : self);
    }
}
