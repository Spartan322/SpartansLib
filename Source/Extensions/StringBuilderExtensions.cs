using System;
using System.Globalization;
using System.Text;

namespace SpartansLib.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder ToUpper(this StringBuilder builder, CultureInfo info = null)
        {
            if (info == null) info = CultureInfo.InvariantCulture;
            for(int i = 0; i < builder.Length; i++)
                builder[i] = char.ToUpper(builder[i], info);
            return builder;
        }

        public static StringBuilder ToLower(this StringBuilder builder, CultureInfo info = null)
        {
            if (info == null) info = CultureInfo.InvariantCulture;
            for (int i = 0; i < builder.Length; i++)
                builder[i] = char.ToLower(builder[i], info);
            return builder;
        }
    }
}
