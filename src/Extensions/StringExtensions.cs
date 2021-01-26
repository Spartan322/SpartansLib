using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace SpartansLib.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveWhitespace(this string input)
            => new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());

        public static string[] SplitWhitespace(this string input, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
            => input.Trim().Split((char[])null, options);

        public static string SpaceCapitals(this string input, Func<char, bool> shouldSpace)
            => input.SpaceCapitals((c, i) => shouldSpace.Invoke(c));

        public static string SpaceCapitals(this string input, Func<char, int, bool> shouldSpace)
        {
            var newStr = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length; i++)
                if (char.IsUpper(input[i]) && i > 0 && !char.IsWhiteSpace(input[i-1]) && shouldSpace(input[i], i))
                        newStr.Append(' ' + input[i].ToString());
                else newStr.Append(input[i]);
            return newStr.ToString();
        }

        public static string SpaceCapitals(this string input)
            => input.SpaceCapitals((c, i) => true);

        private static Exception CreateArgParseFail(string val, string parseToName, string valVarName)
            => new ArgumentException($"Can't convert {val} to {parseToName}", valVarName);

        public static Vector2 AsVec2(this string input)
        {
            if (input.AsVec2(out var result))
                return result;
            throw CreateArgParseFail(input, "Vector2", nameof(input));
        }

        public static bool AsVec2(this string input, out Vector2 result)
        {
            result = new Vector2();
            if (!input.Contains(","))
                return false;
            input = input.Replace('(', ' ').RemoveWhitespace();
            var split = input.Split(',');
            if (split.Length != 2)
                return false;
            if (float.TryParse(split[0], out var x) && float.TryParse(split[1], out var y))
            {
                result = new Vector2(x, y);
                return true;
            }
            return false;
        }

        public static Vector3 AsVec3(this string input)
        {
            if (input.AsVec3(out var result))
                return result;
            throw CreateArgParseFail(input, "Vector3", nameof(input));
        }


        public static bool AsVec3(this string input, out Vector3 result)
        {
            result = new Vector3();
            if (!input.Contains(","))
                return false;
            input = input.Replace('(', ' ').RemoveWhitespace();
            var split = input.Split(',');
            if (split.Length != 3)
                return false;
            if (float.TryParse(split[0], out var x) && float.TryParse(split[1], out var y) && float.TryParse(split[2], out var z))
            {
                result = new Vector3(x, y, z);
                return true;
            }
            return false;
        }

        public static Rect2 AsRect2(this string input)
        {
            if (input.AsRect2(out var result))
                return result;
            throw CreateArgParseFail(input, "Rect2", nameof(input));
        }


        public static bool AsRect2(this string input, out Rect2 result)
        {
            result = new Rect2();
            if (!input.Contains(","))
                return false;
            input = input.Replace('(', ' ').RemoveWhitespace();
            var split = input.Split(',');
            if (split.Length != 4)
                return false;
            if (float.TryParse(split[0], out var x) && float.TryParse(split[1], out var y) && float.TryParse(split[2], out var w) && float.TryParse(split[3], out var h))
            {
                result = new Rect2(x, y, w, h);
                return true;
            }
            return false;
        }
    }
}
