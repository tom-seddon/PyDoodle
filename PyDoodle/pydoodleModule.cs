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

        public ObjectOperations ObjectOperations { get { return _operations; } }

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
            ss.SetVariable("Attr", DynamicHelpers.GetPythonTypeFromType(typeof(Attr)));
            ss.SetVariable("TranslateHandle", DynamicHelpers.GetPythonTypeFromType(typeof(TranslateHandle)));

            _drawStateStack = new Stack<DrawState>();
            _drawStateStack.Push(new DrawState());

            _operations = se.CreateOperations();
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

        public delegate void tweaknType(CodeContext context, params Attr[] attrs);
        public void tweakn(CodeContext context, params Attr[] attrs)
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

        public class TranslateHandle : Handle
        {
            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            private Matrix _transform;
            private bool _dragging;
            private V2 _dragDelta;
            private float _radius;
            private bool _hot;

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public TranslateHandle()
            {
                _radius = 10;//in pixels.
                _transform = null;
                _dragging = false;
                _hot = false;
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            /// <summary>
            /// call tick() from python code. It stores the current graphics
            /// transform and adds the handle to the list of handles to be
            /// active this frame.
            /// </summary>
            public void tick()
            {
                Graphics g = this.Attr.Module.Graphics;
                GraphicsControl gc = this.Attr.Module.GraphicsControl;

                if (g != null && gc != null)
                {
                    _transform = g.Transform;

                    gc.AddHandle(this);
                }
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public override void Draw(Graphics g)
            {
                if (_transform == null)
                    return;

                PointF screenPos = Misc.TransformPoint(_transform, (V2)this.Attr.GetValue()).AsPointF();

                GraphicsState gs = g.Save();

                g.ResetClip();
                g.ResetTransform();

                Color colour;
                if (_hot)
                    colour = Color.LightGray;
                else
                    colour = Color.Black;

                g.FillEllipse(new SolidBrush(colour), screenPos.X - _radius, screenPos.Y - _radius, _radius * 2, _radius * 2);

                g.Restore(gs);
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public override bool HandleMouseEvent(object sender, MouseEventArgs mea, V2 mouseScenePos)
            {
                if (_transform == null)
                {
                    _dragging = false;
                    return false;
                }

                V2 mouseScreenPos = new V2(mea.Location.X, mea.Location.Y);

                bool hover = false;
                V2 delta = V2.V00;

                if (!_dragging)
                {
                    V2 screenPos = Misc.TransformPoint(_transform, (V2)this.Attr.GetValue());

                    delta = mouseScreenPos - screenPos;

                    if (delta.GetLengthSq() < _radius * _radius)
                        hover = true;
                }

                if ((mea.Button & MouseButtons.Left) != 0)
                {
                    if (!_dragging)
                    {
                        if (hover)
                        {
                            _dragDelta = delta;
                            _dragging = true;
                        }
                    }
                    else
                    {
                        Matrix invTransform = _transform.Clone();
                        invTransform.Invert();

                        V2 screenPos = mouseScreenPos + _dragDelta;
                        V2 scenePos = Misc.TransformPoint(invTransform, screenPos);

                        this.Attr.SetValue(scenePos);
                    }
                }
                else
                {
                    _dragging = false;
                }

                if (hover)
                    _hot = true;
                else if (_dragging)
                    _hot = true;
                else
                    _hot = false;

                return _dragging;
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