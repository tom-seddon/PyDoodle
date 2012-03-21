using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using System.Windows.Forms;
using System.Drawing;
using IronPython.Runtime;
using System.Drawing.Drawing2D;

namespace PyDoodle
{
    public class pydoodleModule
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private MainForm _mainForm;
        private Graphics _graphics;
        private ObjectOperations _operations;
        private GraphicsControl _graphicsControl;

        private static readonly Matrix identityMatrix = new Matrix();

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public GraphicsControl GraphicsControl
        {
            get { return _graphicsControl; }
        }
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public Graphics Graphics
        {
            get { return _graphics; }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private Color _drawColour;
        private Pen _drawPen;
        private Brush _drawBrush;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public ObjectOperations ObjectOperations { get { return _operations; } }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        // Attrs come from a pool, so that there's (mostly) one Attr per Python
        // variable.
        private List<Attr> _attrPool;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void BeginDraw(Graphics graphics, GraphicsControl graphicsControl)
        {
            _graphics = graphics;
            _graphicsControl = graphicsControl;

            set_colour(new Colour(0f, 0f, 0f, 1f));
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void EndDraw()
        {
            _graphics = null;
            _graphicsControl = null;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public pydoodleModule(ScriptEngine se, MainForm mainForm)
        {
            _mainForm = mainForm;

            ScriptScope ss = Python.CreateModule(se, "pydoodle");

            ss.SetVariable("run", new Action<object>(this.run));
            ss.SetVariable("V2", DynamicHelpers.GetPythonTypeFromType(typeof(V2)));

            ss.SetVariable("line", new Action<V2, V2>(this.line));

            ss.SetVariable("circle", new Action<V2, float>(this.circle));
            ss.SetVariable("fcircle", new Action<V2, float>(this.fcircle));

            ss.SetVariable("ellipse", new Action<V2, float, float>(this.ellipse));
            ss.SetVariable("fellipse", new Action<V2, float, float>(this.fellipse));

            ss.SetVariable("box", new Action<V2, V2>(this.box));
            ss.SetVariable("fbox", new Action<V2, V2>(this.fbox));

            ss.SetVariable("text", new Action<V2, string>(this.text));

            ss.SetVariable("set_colour", new Action<Colour>(this.set_colour));

            ss.SetVariable("tweaks", new Action<Attr[]>(this.tweaks));

            ss.SetVariable("add_translate_handles", new Action<Attr[]>(this.add_translate_handles));
            ss.SetVariable("add_rotate_handles", new Action<Attr[]>(this.add_rotate_handles));

            //ss.SetVariable("TranslateHandle", DynamicHelpers.GetPythonTypeFromType(typeof(TranslateHandle)));
            ss.SetVariable("attr", new Func<object, string, Attr>(this.attr));
            ss.SetVariable("attrs", new attrsType(this.attrs));

            {
                PythonType colourType = DynamicHelpers.GetPythonTypeFromType(typeof(Colour));
                ss.SetVariable("Colour", colourType);
                ss.SetVariable("Color", colourType);
            }

            _operations = se.CreateOperations();

            _attrPool = new List<Attr>();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// returns Attr for the given attribute on the given object.
        /// </summary>
        /// <param name="pyobj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Attr attr(object pyobj, string name)
        {
            Attr newAttr = new Attr(this, pyobj, name);

            foreach (Attr attr in _attrPool)
            {
                if (attr.IsEquivalent(newAttr))
                    return attr;
            }

            _attrPool.Add(newAttr);
            return newAttr;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private delegate Attr[] attrsType(object pyobj, params string[] names);

        /// <summary>
        /// Returns list of Attr, one for each mentioned attribute on the given
        /// object.
        /// </summary>
        /// <param name="pyobj"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        private Attr[] attrs(object pyobj, params string[] names)
        {
            Attr[] attrs = new Attr[names.Length];

            for (int i = 0; i < names.Length; ++i)
                attrs[i] = attr(pyobj, names[i]);

            return attrs;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void set_colour(Colour colour)
        {
            _drawColour = colour.AsColor();
            _drawPen = new Pen(_drawColour);
            _drawBrush = new SolidBrush(_drawColour);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void line(V2 a, V2 b)
        {
            if (_graphics == null)
                return;

            _graphics.DrawLine(_drawPen, (float)a.x, (float)a.y, (float)b.x, (float)b.y);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void circle(V2 c, float r)
        {
            ellipse(c, r, r);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void fcircle(V2 c, float r)
        {
            fellipse(c, r, r);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void ellipse(V2 c, float rx, float ry)
        {
            if (_graphics == null)
                return;

            RectangleF rect = new RectangleF((float)c.x - rx, (float)c.y - ry, rx * 2, ry * 2);

            _graphics.DrawEllipse(_drawPen, rect);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void fellipse(V2 c, float rx, float ry)
        {
            if (_graphics == null)
                return;

            RectangleF rect = new RectangleF((float)c.x - rx, (float)c.y - ry, rx * 2, ry * 2);

            _graphics.FillEllipse(_drawBrush, rect);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private RectangleF GetBoxRect(V2 a, V2 b)
        {
            V2 d = b - a;

            return new RectangleF((float)Math.Min(a.x, b.x), (float)Math.Min(a.y, b.y), (float)Math.Abs(d.x), (float)Math.Abs(d.y));
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void box(V2 a, V2 b)
        {
            if (_graphics == null)
                return;

            RectangleF rect = GetBoxRect(a, b);
            _graphics.DrawRectangle(_drawPen, rect.Left, rect.Top, rect.Width, rect.Height);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void fbox(V2 a, V2 b)
        {
            if (_graphics == null)
                return;

            _graphics.FillRectangle(_drawBrush, GetBoxRect(a, b));
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void text(V2 pos, string text)
        {
            if (_graphics == null)
                return;

            Matrix oldTransform = _graphics.Transform;

            _graphics.Transform = identityMatrix;

            V2 screenPos = Misc.TransformPoint(oldTransform, pos);

            _graphics.DrawString(text, SystemFonts.DefaultFont, _drawBrush, screenPos.AsPointF());

            _graphics.Transform = oldTransform;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void AddHandle(Handle handle)
        {
            _operations.SetMember(handle.Attr.Pyobj, handle.Attr.Name + "_handle", handle);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void add_translate_handles(Attr[] attrs)
        {
            foreach (Attr attr in attrs)
                AddHandle(new TranslateHandle(attr));
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void add_rotate_handles(Attr[] attrs)
        {
            foreach (Attr attr in attrs)
                AddHandle(new RotateHandle(attr));
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds tweaks as appropriate for the given list of attributes.
        /// </summary>
        /// <param name="attrs"></param>
        private void tweaks(Attr[] attrs)
        {
            for (int i = 0; i < attrs.Length; ++i)
            {
                Attr attr = attrs[i];

                TweakControl tweakControl = null;

                object attrValue = attr.GetValue();

                // Check...
                if (attrValue is string)
                    tweakControl = new TweakStringControl(attr);
                else if (attrValue is V2)
                    tweakControl = new TweakV2Control(attr);
                else if (attrValue is float || attrValue is double)
                    tweakControl = new TweakFloatControl(attr);
                else
                    throw new ArgumentException(string.Format("Attr {0} - no tweaker for type {1}.", attr.Name, attrValue.GetType()));

                _mainForm.TweaksPanel.AddTweakControl(tweakControl);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void run(object runPyobj)
        {
            _mainForm.RunPyobj = runPyobj;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}