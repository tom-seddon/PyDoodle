using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;

namespace PyDoodle
{
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

        public TranslateHandle(Attr attr)
            : base(attr)
        {
            if (!(attr.GetValue() is V2))
                throw new ArgumentException("Attr for TranslateHandle must be V2, not "+attr.GetValue().GetType());

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

                if (delta.lensq() < _radius * _radius)
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
}
