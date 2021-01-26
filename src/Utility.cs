using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace SpartansLib
{
    public static class Utility
    {
        public static float Normalize(float value, float start, float end)
        {
            var width = end - start;   //
            var offsetValue = value - start;   // value relative to 0

            return (offsetValue - (Mathf.Floor(offsetValue / width) * width)) + start;
            // + start to reset back to start of original range
        }

        public static List<(string parsedString, bool endStatement)> ParseLineSeperator(this List<string> parsedStrings, string seperator = ";\n", string escape = "\\", List<int> ignoreInds = null)
        {
            var tokens = new List<(string, bool)>();
            var isEscaping = false;
            var frag = new StringBuilder(100);

            for (var i = 0; i < parsedStrings.Count; i++)
            {
                if (ignoreInds.Contains(i))
                {
                    frag.Append(parsedStrings[i]);
                    Flush(false);
                    continue;
                }
                var args = parsedStrings[i];
                foreach(var c in args)
                {
                    if(isEscaping)
                    {
                        frag.Append(c);
                        isEscaping = false;
                    }
                    else
                    {
                        if (escape.Contains(c))
                        {
                            isEscaping = true;
                            continue;
                        }
                        if (seperator.Contains(c))
                            Flush(true);
                        frag.Append(c);
                    }
                }
            }
            Flush(false);

            return tokens;

            void Flush(bool endOfStatement)
            {
                if (frag.Length != 0)           // if fragment has a length
                {
                    var fragStr = frag.ToString().Trim();
                    if (fragStr.Length != 0)        // if fragment doesn't contain only whitespace
                        tokens.Add((fragStr,endOfStatement));            // add to tokens with trimmed whitespace
                }
                frag.Clear();                   // clear fragment
            }
        }

        public static bool ContainsAny(this string input, string check)
        {
            foreach (var c in input)
                if (check.Contains(c))
                    return true;
            return false;
        }

        private enum ParseMove
        {
            Check, Escape, Capture
        }

        public static List<string> ParseEscapableString(this string text, string capture = "\"", string end = " \n\t", string escape = "\\")
            => text.ParseEscapableString(capture, end, escape, out var array);

        public static List<string> ParseEscapableString(this IEnumerable<char> text, string capture, string end, string escape, out List<int> capturedInds)
        {
            capturedInds = new List<int>();
            var tokens = new List<string>();
            var move = ParseMove.Check;
            var capturing = false;
            var frag = new StringBuilder(100);

            foreach(var c in text)
            {
                switch(move)
                {
                    case ParseMove.Check:           // check
                        if (escape.Contains(c))         // if escape was found
                            move = ParseMove.Escape;        // skip escape character
                        else if (!capture.Contains(c))  // if capture wasn't found
                        {
                            if (end.Contains(c))            // if end was found
                            {
                                Flush();
                                break;
                            }
                            frag.Append(c);                 // if end was not found read c
                        }
                        else
                        {
                            Flush();
                            move = ParseMove.Capture;  // else skip capture character
                        }
                        break;
                    case ParseMove.Escape:          // escape character
                        frag.Append(c);
                        move = capturing ? ParseMove.Capture : ParseMove.Check;
                        break;
                    case ParseMove.Capture:         // read capture
                        capturing = true;
                        if (escape.Contains(c))         // if escape found
                            move = ParseMove.Escape;
                        else if (!capture.Contains(c))  // if capture not found
                            frag.Append(c);                 // read c
                        else                            // if capture found
                        {
                            capturedInds.Add(tokens.Count);
                            Flush();
                            capturing = false;
                            move = ParseMove.Check;
                        }
                        break;
                }
            }
            Flush();
            return tokens;

            void Flush()
            {
                if (frag.Length != 0)           // if fragment has a length
                {
                    var fragStr = frag.ToString().Trim();
                    if (fragStr.Length != 0)        // if fragment doesn't contain only whitespace
                        tokens.Add(fragStr);            // add to tokens with trimmed whitespace
                }
                frag.Clear();                   // clear fragment
            }
        }

        public static List<string> SerialSplit(this string text, char selector, string enders = "\\w", char esc = '\\')
        {
            var tokens = new List<string>();
            byte move = 0; // 0 normal, 1 group capture, 2 escape
            var frag = new StringBuilder(100);
            var checkForWhitespace = enders == "\\w";

            for (var i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (move)
                {
                    case 0:
                        if (c == selector)
                        {
                            var nextEnder = checkForWhitespace ?
                                text.IndexOfAny(new char[] { ' ', '\t', '\n' }, i)
                                : text.IndexOfAny(enders.ToCharArray(), i);
                            var nextSelector = text.IndexOf(selector, i + 1);
                            if (text.IndexOf(selector, i + 1) == -1
                                // if second capture selector exists
                                || (nextEnder > -1
                                    && nextEnder < nextSelector)
                                // if voider of capture
                                || text.IndexOf(esc, i) == nextSelector - 1)
                                // if capture is escaped
                                frag.Append(c); // continue seeking
                            else
                            {
                                // execute group capture
                                if (frag.Length != 0) tokens.Add(frag.ToString());
                                frag.Clear();
                                move = 1;
                            }
                        }
                        else if (c == esc) move = 2; // escape character
                        else frag.Append(c); // continue seeking
                        break;
                    case 1:
                        if (c == selector)
                        {
                            // end group capture
                            if (frag.Length != 0) tokens.Add(frag.ToString());
                            frag.Clear();
                            move = 0;
                        }
                        else frag.Append(c); // continue capture
                        break;
                    case 2:
                        // end escape
                        frag.Append(c);
                        move = 0;
                        break;
                }
            }
            if (frag.Length != 0)
                tokens.Add(frag.ToString());
            return tokens;
        }

        public static bool HasFlagOf<EnumT>(this EnumT @enum, EnumT check)
            where EnumT : Enum
        {
            var valCheck = Convert.ToUInt64(check);
            var has = (Convert.ToUInt64(@enum) & valCheck) != 0;
            return has;
        }

        public static bool TargetsMembers(this AttributeTargets targets)
            => targets.HasFlagOf(
                    AttributeTargets.Property
                        | AttributeTargets.Field
                        | AttributeTargets.Method
                        | AttributeTargets.Event
                        | AttributeTargets.Constructor);

        public static bool TargetsTypes(this AttributeTargets targets)
            => targets.HasFlagOf(
                    AttributeTargets.Class
                        | AttributeTargets.Interface
                        | AttributeTargets.Struct);

        public static Color GetColorFor<T>(this T node)
            where T : Node
        {
            if(node is CanvasItem canvasItem)
            {
                if (canvasItem is Polygon2D polygon2d)
                    return polygon2d.Color;
                return canvasItem.Modulate;
            }
            return default;
        }
    }
}
