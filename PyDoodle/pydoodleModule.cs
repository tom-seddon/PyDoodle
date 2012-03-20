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
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private MainForm _mainForm;
        private Graphics _graphics;
        private ObjectOperations _operations;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        public Graphics Graphics
        {
            get { return _graphics; }
            set { _graphics = value; }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
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

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        private Stack<DrawState> _drawStateStack;

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
            ss.SetVariable("push_draw_state", new Action(this.push_draw_state));
            ss.SetVariable("pop_draw_state", new Action(this.push_draw_state));
            ss.SetVariable("set_draw_colour_rgba", new Action<float, float, float, float>(this.set_draw_colour_rgba));
            ss.SetVariable("set_draw_colour_rgb", new Action<float, float, float>(this.set_draw_colour_rgb));
            ss.SetVariable("tweakn", new tweaknType(this.tweakn));
            //ss.SetVariable("handle", new handleDelegate(this.handle));
            ss.SetVariable("ref_test",new ref_testDelegate(this.ref_test));

            _drawStateStack = new Stack<DrawState>();
            _drawStateStack.Push(new DrawState());

            _operations = se.CreateOperations();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        private delegate void ref_testDelegate(ref float f, ref V2 v);
        private void ref_test(ref float f, ref V2 v)
        {
            Console.WriteLine("ref_test: in: f={0} v={1}", f, v);

            f += 1f;
            v.x += 1.0;
            v.y += 1.0;

            Console.WriteLine("ref_test: out: f={0} v={1}", f, v);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        private void set_draw_colour_rgb(float r, float g, float b)
        {
            set_draw_colour_rgba(r, g, b, 1.0f);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        private static int GetColourByte(float v)
        {
            if (v < .0f)
                return 0;
            else if (v > 1.0f)
                return 255;
            else
                return (int)(v * 255.0f);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void push_draw_state()
        {
            DrawState tos = (DrawState)_drawStateStack.Peek().Clone();
            _drawStateStack.Push(tos);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void pop_draw_state()
        {
            _drawStateStack.Pop();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void set_draw_colour_rgba(float r, float g, float b, float a)
        {
            int ia = GetColourByte(a);
            int ir = GetColourByte(r);
            int ig = GetColourByte(g);
            int ib = GetColourByte(b);

            _drawStateStack.Peek().Colour = Color.FromArgb(ia, ir, ig, ib);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void line(V2 a, V2 b)
        {
            if (_graphics == null)
                return;

            Pen pen = _drawStateStack.Peek().Pen;
            _graphics.DrawLine(pen, (float)a.x, (float)a.y, (float)b.x, (float)b.y);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void circle(V2 c, float r)
        {
            if (_graphics == null)
                return;

            RectangleF rect = new RectangleF((float)c.x - r, (float)c.y - r, r * 2, r * 2);

            Pen pen = _drawStateStack.Peek().Pen;
            _graphics.DrawEllipse(pen, rect);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public delegate void tweaknType(CodeContext context, object pyobj, params string[] attrs);
        public void tweakn(CodeContext context, object pyobj, params object[] attrs)
        {
            for (int i = 0; i < attrs.Length; ++i)
            {
                int argIdx = 1 + i;
                string attrName = null;
                PythonDictionary attrDict = null;

                // Pull out 
                if (attrs[i] is string)
                    attrName = (string)attrs[i];
                else if (attrs[i] is PythonTuple)
                {
                    PythonTuple tuple = (PythonTuple)attrs[i];

                    switch (tuple.Count)
                    {
                    default:
                        throw new ArgumentException("Argument {0} - must be (str,) or (str,dict).");

                    case 1:
                        {
                            if (!(tuple[0] is string))
                                goto default;

                            attrName = (string)tuple[0];
                        }
                        break;

                    case 2:
                        {
                            if (!(tuple[1] is PythonDictionary))
                                goto default;

                            attrDict = (PythonDictionary)tuple[1];
                        }
                        goto case 1;
                    }
                }
                else
                    throw new ArgumentException(string.Format("Argument {0} - unsupported type, {1}.", argIdx, attrs[i].GetType()));

                TweakControl.Creator creator = null;

                dynamic attrValue = _operations.GetMember(pyobj, attrName);

                // Check...
                if (attrValue is string)
                    creator = TweakControl.CreateTweakControl<TweakStringControl>;
                else if (attrValue is V2)
                    creator = TweakControl.CreateTweakControl<TweakV2Control>;
                else if (attrValue is float || attrValue is double)
                    creator = TweakControl.CreateTweakControl<TweakFloatControl>;
                else
                    throw new ArgumentException(string.Format("Attribute {0} - unsupported type, {1}.", attrName, attrName.GetType()));

                _mainForm.TweaksPanel.AddTweak(pyobj, attrName, creator);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

//         private delegate void handleDelegate(CodeContext context, object pyobj, params string[] attrs);
//         private void handle(CodeContext context, object pyobj, params string[] attrs)
//         {
//             if (_graphics == null)
//                 return;
// 
//             foreach (string attr in attrs)
//             {
//                 dynamic attrValue = _operations.GetMember(pyobj, attrName);
// 
//                 if (attrValue is V2)
//                 {
// 
//                 }
//             }
//         }

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
