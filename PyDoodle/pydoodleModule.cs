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

namespace PyDoodle
{
    public class pydoodleModule
    {
        private MainForm _mainForm;
        private Graphics _graphics;
        private ObjectOperations _operations;

        public Graphics Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        class DrawState : ICloneable
        {
            private Color _colour;
            private float _width;
            private Pen _pen;

            public DrawState()
            {
                _colour = Color.Black;
                _width = 1.0f;

                _pen = null;
            }

            public object Clone()
            {
                DrawState clone = new DrawState();

                clone._colour = _colour;
                clone._width = _width;

                return clone;
            }

            public Pen Pen
            {
                get
                {
                    if (_pen == null || _pen.Color != _colour || _pen.Width != _width)
                        _pen = new Pen(_colour, _width);

                    return _pen;
                }
            }

            public Color Colour
            {
                get { return _colour; }
                set { _colour = value; }
            }

            public float Width
            {
                get { return _width; }
                set { _width = value; }
            }
        }

        private Stack<DrawState> _drawStateStack;

        public pydoodleModule(ScriptEngine se, MainForm mainForm)
        {
            _mainForm = mainForm;

            ScriptScope ss = Python.CreateModule(se, "pydoodle");

            ss.SetVariable("run", new Action<object>(this.run));
            ss.SetVariable("V2", DynamicHelpers.GetPythonTypeFromType(typeof(V2)));
            ss.SetVariable("line", new Action<V2, V2>(this.line));
            ss.SetVariable("circle", new Action<V2, float>(this.circle));
            ss.SetVariable("push_draw_state", new Action(this.push_draw_state));
            ss.SetVariable("pop_draw_state", new Action(this.push_draw_state));
            ss.SetVariable("set_draw_colour_rgba", new Action<float, float, float, float>(this.set_draw_colour_rgba));
            ss.SetVariable("set_draw_colour_rgb", new Action<float, float, float>(this.set_draw_colour_rgb));
            ss.SetVariable("tweak", new tweak_type(this.tweak));

            _drawStateStack = new Stack<DrawState>();
            _drawStateStack.Push(new DrawState());

            _operations = se.CreateOperations();
        }

        private void set_draw_colour_rgb(float r, float g, float b)
        {
            set_draw_colour_rgba(r, g, b, 1.0f);
        }

        private static int GetColourByte(float v)
        {
            if (v < .0f)
                return 0;
            else if (v > 1.0f)
                return 255;
            else
                return (int)(v * 255.0f);
        }

        private void push_draw_state()
        {
            DrawState tos = (DrawState)_drawStateStack.Peek().Clone();
            _drawStateStack.Push(tos);
        }

        private void pop_draw_state()
        {
            _drawStateStack.Pop();
        }

        private void set_draw_colour_rgba(float r, float g, float b, float a)
        {
            int ia = GetColourByte(a);
            int ir = GetColourByte(r);
            int ig = GetColourByte(g);
            int ib = GetColourByte(b);

            _drawStateStack.Peek().Colour = Color.FromArgb(ia, ir, ig, ib);
        }

        public void line(V2 a, V2 b)
        {
            if (_graphics == null)
                return;

            Pen pen = _drawStateStack.Peek().Pen;
            _graphics.DrawLine(pen, (float)a.x, (float)a.y, (float)b.x, (float)b.y);
        }

        public void circle(V2 c, float r)
        {
            if (_graphics == null)
                return;

            RectangleF rect = new RectangleF((float)c.x - r, (float)c.y - r, r * 2, r * 2);

            Pen pen = _drawStateStack.Peek().Pen;
            _graphics.DrawEllipse(pen, rect);
        }

        public delegate void tweak_type(CodeContext context, object pyobj, params string[] attrs);
        public void tweak(CodeContext context, object pyobj, params string[] attrs)
        {
            for (int i = 0; i < attrs.Length; ++i)
            {
                dynamic attr = _operations.GetMember(pyobj, attrs[i]);

                // Check...
                if (attr is string)
                {
                    _mainForm.TweaksPanel.AddTweak(pyobj, attrs[i], TweakControl.CreateTweakControl<TweakStringControl>);
                }
                else if (attr is V2)
                {
                    _mainForm.TweaksPanel.AddTweak(pyobj, attrs[i], TweakControl.CreateTweakControl<TweakV2Control>);
                }
                else
                {
                    Console.WriteLine("    unhandled type {0} (attr {1}, obj {2})", attr.GetType(), attrs[i], pyobj);
                }
            }
//             Console.WriteLine("tweak: pyobj={0}", pyobj);
//             for (int i = 0; i < attrs.Length; ++i)
//             {
//                 Console.WriteLine("    attrs[{0}]=\"{1}\"", i, attrs[i]);
// 
//                 dynamic attr = _operations.GetMember(pyobj, attrs[i]);
//                 Console.WriteLine("        type: {0}", attr.GetType());
// 
//                 if (attr is V2)
//                     Console.WriteLine("        (it's a V2)");
//             }
        }

        private void run(object runPyobj)
        {
            _mainForm.RunPyobj = runPyobj;
        }
    }
}
