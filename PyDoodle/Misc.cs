using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;
using IronPython.Hosting;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Scripting.Runtime;
using System.Security;
using System.Drawing.Drawing2D;

namespace PyDoodle
{
    public static class Misc
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static bool AreFileNamesEqual(string fileNameA, string fileNameB)
        {
            string a = fileNameA.Replace('/', '\\').ToLower();
            string b = fileNameB.Replace('/', '\\').ToLower();

            if (a != b)
                return false;

            return true;
        }

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

        private static void SetTextBoxesColour(TextBox[] textBoxes, Color colour)
        {
            foreach (TextBox t in textBoxes)
            {
                if (t != null)
                    t.BackColor = colour;
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static void SetCleanColour(params TextBox[] textBoxes)
        {
            SetTextBoxesColour(textBoxes, System.Drawing.SystemColors.Window);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static void SetDirtyColour(params TextBox[] textBoxes)
        {
            SetTextBoxesColour(textBoxes, Control.DefaultBackColor);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static void SetBadColour(params TextBox[] textBoxes)
        {
            double h, s, v;
            Misc.ColorToHSV(TextBox.DefaultBackColor, out h, out s, out v);

            h = 0;//red
            s = 0.25;

            SetTextBoxesColour(textBoxes, Misc.ColorFromHSV(h, s, v));
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static readonly Encoding defaultXmlEncoding = new UTF8Encoding(false);//false=no BOM

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static T LoadXml<T>(string fileName) where T : class
        {
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(reader) as T;
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static T LoadXmlOrCreateDefault<T>(string fileName) where T : class,new()
        {
            try
            {
                return LoadXml<T>(fileName);
            }
            catch (InvalidOperationException)
            {
            }
            catch (FileNotFoundException)
            {
            }

            return new T();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static void SaveXml<T>(string fileName, T data)
        {
            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Encoding = defaultXmlEncoding;
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, data);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static void Dispose(object o)
        {
            IDisposable disposable = o as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static string[] TryReadAllLines(string fileName)
        {
            try
            {
                return File.ReadAllLines(fileName);
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (SecurityException)
            {
            }

            return new string[] { };
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static string GetScriptExceptionDynamicStackFramesTrace(Exception e)
        {
            var sourceFiles = new Dictionary<string, string[]>(FileNamesComparer.instance);
            var stackFrames = GetScriptExceptionDynamicStackFrames(e);
            var builder = new StringBuilder();

            foreach (var stackFrame in stackFrames)
            {
                string fileName = stackFrame.GetFileName();
                int lineNumber = stackFrame.GetFileLineNumber();

                string[] fileContents;
                if (!sourceFiles.TryGetValue(fileName, out fileContents))
                {
                    fileContents = TryReadAllLines(fileName);
                    sourceFiles[fileName] = fileContents;
                }

                builder.AppendFormat("{0}({1})", fileName, lineNumber);

                if (fileContents != null && lineNumber - 1 < fileContents.Length)
                    builder.AppendFormat(": {0}", fileContents[lineNumber - 1]);

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static List<DynamicStackFrame> GetScriptExceptionDynamicStackFrames(Exception e)
        {
            List<DynamicStackFrame> stackFrames = new List<DynamicStackFrame>();

            Type key = typeof(DynamicStackFrame);
            if (e.Data.Contains(key))
            {
                var exceptionStackFrames = e.Data[key] as List<DynamicStackFrame>;
                if (exceptionStackFrames != null)
                {
                    foreach (DynamicStackFrame stackFrame in exceptionStackFrames)
                        stackFrames.Add(stackFrame);
                }
            }

            return stackFrames;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public static V2 TransformPoint(Matrix transform, V2 point)
        {
            PointF[] pointArray = { new PointF((float)point.x, (float)point.y) };
            transform.TransformPoints(pointArray);

            return new V2(pointArray[0].X, pointArray[0].Y);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }

    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// IEqualityComparer for filenames.
    /// 
    /// This is not especially efficient.
    /// </summary>
    public class FileNamesComparer :
        IEqualityComparer<string>,
        IComparer<string>
    {
        private static String GetNormalizedFilename(String filename)
        {
            return filename.Replace('/', '\\').ToLower();
        }

        public bool Equals(string a, string b)
        {
            string anorm = GetNormalizedFilename(a);
            string bnorm = GetNormalizedFilename(b);

            return anorm.Equals(bnorm);
        }

        public int Compare(string a, string b)
        {
            string anorm = GetNormalizedFilename(a);
            string bnorm = GetNormalizedFilename(b);

            return string.Compare(anorm, bnorm);
        }

        public int GetHashCode(string obj)
        {
            return GetNormalizedFilename(obj).GetHashCode();
        }

        public static readonly FileNamesComparer instance = new FileNamesComparer();

        /// <summary>
        /// returns a new predicate that checks whether its argument seems to
        /// refer to the same filename as that passed in to generate the
        /// predicate.
        /// 
        /// I think this deserves a better name but I don't know what.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Predicate<string> GetFilenamePredicate(string filename)
        {
            string filenamenorm = GetNormalizedFilename(filename);

            return delegate(string f)
            {
                string fnorm = GetNormalizedFilename(f);

                return fnorm.Equals(filenamenorm, StringComparison.CurrentCultureIgnoreCase);
            };
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }

    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////
}
