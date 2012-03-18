using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;
using IronPython.Hosting;

namespace PyDoodle
{
    public static class Misc
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static Color TransformedColour(Color colour, double dh, double ds, double dv)
        {
            double h, s, v;
            ColorToHSV(colour, out h, out s, out v);

            h += dh;

            s += ds;
            if (s < 0)
                s = 0;
            else if (s > 1)
                s = 1;

            v += dv;
            if (v < 0)
                v = 0;
            else if (v > 1)
                v = 1;

            return ColorFromHSV(h, s, v);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static string GetPathDirectoryName(string path)
        {
            if (path != "")
            {
                try
                {
                    return Path.GetDirectoryName(path);
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static string GetPathFileName(string path)
        {
            if (path != "")
            {
                try
                {
                    return Path.GetFileName(path);
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static List<string> GetModuleFileNames(ScriptEngine se)
        {
            List<string> moduleFileNames = new List<string>();

            PythonDictionary modules = se.GetSysModule().GetVariable("modules") as PythonDictionary;
            if (modules != null)
            {
                foreach (KeyValuePair<object, object> kvp in modules)
                {
                    PythonModule module = kvp.Value as PythonModule;

                    if (module != null)
                    {
                        object fileNameObj;
                        if (module.Get__dict__().TryGetValue("__file__", out fileNameObj))
                        {
                            string fileName = fileNameObj as string;
                            if (fileName != null && fileName.Length > 0)
                                moduleFileNames.Add((string)fileName);
                        }
                    }
                }
            }

            return moduleFileNames;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}
