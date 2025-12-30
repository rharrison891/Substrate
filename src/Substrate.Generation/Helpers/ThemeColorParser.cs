using System;
using System.Globalization;
using System.Linq;

namespace Substrate.Generation.Core
{
    internal static class ThemeColorParser
    {
        public static (bool ok, string key, int a, int r, int g, int b)
            Parse(string key, string value)
        {
            key = key.Trim().Trim('"');
            value = value.Trim().Trim('"');

            if (value.StartsWith("#"))
                value = value.Substring(1);

            int a = 255, r = 0, g = 0, b = 0;

            // #RRGGBB
            if (value.Length == 6)
            {
                r = Convert.ToInt32(value.Substring(0, 2), 16);
                g = Convert.ToInt32(value.Substring(2, 2), 16);
                b = Convert.ToInt32(value.Substring(4, 2), 16);
                return (true, key, a, r, g, b);
            }

            // #AARRGGBB
            if (value.Length == 8)
            {
                a = Convert.ToInt32(value.Substring(0, 2), 16);
                r = Convert.ToInt32(value.Substring(2, 2), 16);
                g = Convert.ToInt32(value.Substring(4, 2), 16);
                b = Convert.ToInt32(value.Substring(6, 2), 16);
                return (true, key, a, r, g, b);
            }

            return (false, key, 0, 0, 0, 0);
        }
    }

    internal static class ThemeDefaults
    {
        public static readonly List<(string Key, int A, int R, int G, int B)> BasePalette =
            new()
            {
            ("Background", 255, 30, 30, 30),
            ("Foreground", 255, 220, 220, 220),
            ("Primary",    255, 0, 120, 215),
            ("Secondary",  255, 45, 45, 48),
            ("Accent",     255, 0, 153, 204),
            ("Border",     255, 90, 90, 90),
            ("Error",      255, 232, 17, 35),
            ("Warning",    255, 255, 185, 0),
            ("Success",    255, 16, 124, 16)
            };
    }
}
