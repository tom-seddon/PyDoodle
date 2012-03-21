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

        public pydoodleModule(ScriptEngine se, MainForm mainForm)
        {
            _mainForm = mainForm;

            ScriptScope ss = Python.CreateModule(se, "pydoodle");

            ss.SetVariable("run", new Action<object>(this.run));
            ss.SetVariable("V2", DynamicHelpers.GetPythonTypeFromType(typeof(V2)));
            ss.SetVariable("line", new Action<V2, V2>(this.line));
            ss.SetVariable("circle", new Action<V2, float>(this.circle));
            ss.SetVariable("set_draw_colour", new Action<Colour>(this.set_draw_colour));
            ss.SetVariable("tweakn", new tweaknType(this.tweakn));
            ss.SetVariable("Attr", DynamicHelpers.GetPythonTypeFromType(typeof(Attr)));
            ss.SetVariable("TranslateHandle", DynamicHelpers.GetPythonTypeFromType(typeof(TranslateHandle)));

            {
                PythonType colourType = DynamicHelpers.GetPythonTypeFromType(typeof(Colour));
                ss.SetVariable("Colour", colourType);
                ss.SetVariable("Color", colourType);
            }

            _operations = se.CreateOperations();
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

        public delegate void tweaknType(params Attr[] attrs);
        public void tweakn(params Attr[] attrs)
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
                    throw new ArgumentException(string.Format("Attribute {0} - no tweaker for type {1}.", attr.Name, attrValue.GetType()));

                _mainForm.TweaksPanel.AddTweakControl(tweakControl);

                // Install handler?
                if (attr.Handle != null)
                {
                    if (attr.AutoCreateHandleField)
                        _operations.SetMember(attr.Pyobj, attr.Name + "_handle", attr.Handle);

                    attr.Handle.Attr = attr;
                }
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