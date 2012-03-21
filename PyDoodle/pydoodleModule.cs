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

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public GraphicsControl GraphicsControl
        {
            get { return _graphicsControl; }
            set { _graphicsControl = value; }
        }
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public Graphics Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
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

        public pydoodleModule(ScriptEngine se, MainForm mainForm)
        {
            _mainForm = mainForm;

            ScriptScope ss = Python.CreateModule(se, "pydoodle");

            ss.SetVariable("run", new Action<object>(this.run));
            ss.SetVariable("V2", DynamicHelpers.GetPythonTypeFromType(typeof(V2)));
            ss.SetVariable("line", new Action<V2, V2>(this.line));
            ss.SetVariable("circle", new Action<V2, float>(this.circle));
            ss.SetVariable("set_draw_colour", new Action<Colour>(this.set_draw_colour));
            ss.SetVariable("tweaks", new Action<Attr[]>(this.tweaks));
            ss.SetVariable("add_translate_handles", new Action<Attr[]>(this.add_translate_handles));
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

            set_draw_colour(new Colour(0f, 0f, 0f, 1f));
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
            Attr newAttr = new Attr(pyobj, name);

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

        private void set_draw_colour(Colour colour)
        {
            _drawColour = colour.AsColor();
            _drawPen = new Pen(_drawColour);
            _drawBrush = new SolidBrush(_drawColour);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void line(V2 a, V2 b)
        {
            if (_graphics == null)
                return;

            _graphics.DrawLine(_drawPen, (float)a.x, (float)a.y, (float)b.x, (float)b.y);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void circle(V2 c, float r)
        {
            if (_graphics == null)
                return;

            RectangleF rect = new RectangleF((float)c.x - r, (float)c.y - r, r * 2, r * 2);

            _graphics.DrawEllipse(_drawPen, rect);
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

        /// <summary>
        /// Adds tweaks as appropriate for the given list of attributes.
        /// </summary>
        /// <param name="attrs"></param>
        private void tweaks(Attr[] attrs)
        {
            for (int i = 0; i < attrs.Length; ++i)
            {
                Attr attr = attrs[i];

                // @TODO: this is a bit mucky...
                attr.Module = this;

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