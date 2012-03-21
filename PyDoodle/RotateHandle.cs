using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace PyDoodle
{
    public class RotateHandle : Handle
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private Matrix _transform;
        private V2 _pos;
        private float _radius;
        private float _handleRadius;
        private bool _dragging;
        private bool _hot;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public RotateHandle(Attr attr)
            : base(attr)
        {
            _transform = null;
            _pos = V2.V00;

            _radius = 25f;
            _handleRadius = 5f;

            _dragging = false;
            _hot = false;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void tick(V2 pos)
        {
            Graphics g = this.Attr.Module.Graphics;
            GraphicsControl gc = this.Attr.Module.GraphicsControl;

            if (g != null && gc != null)
            {
                _transform = g.Transform;
                _pos = pos;

                gc.AddHandle(this);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private bool GetPositions(out PointF screenPos, out PointF handlePos, out double thetaOffset)
        {
            screenPos = PointF.Empty;
            handlePos = PointF.Empty;
            thetaOffset = 0.0;

            if (_transform == null)
                return false;

            screenPos = Misc.TransformPoint(_transform, _pos).AsPointF();

            double theta = Convert.ToDouble(Attr.GetValue());

            float[] elements = _transform.Elements;
            thetaOffset = Math.Atan2(elements[1], elements[0]);

            handlePos = new PointF((float)(screenPos.X + Math.Cos(theta + thetaOffset) * _radius),
                (float)(screenPos.Y + Math.Sin(theta + thetaOffset) * _radius));

            return true;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public override void Draw(Graphics g)
        {
            PointF screenPos, handlePos;
            double thetaOffset;
            if (!GetPositions(out screenPos, out handlePos, out thetaOffset))
                return;

            GraphicsState gs = g.Save();

            g.ResetClip();
            g.ResetTransform();

            Pen pen = new Pen(Color.FromArgb(128, Color.Black));

            g.DrawEllipse(pen, screenPos.X - _radius, screenPos.Y - _radius, _radius * 2f, _radius * 2f);

            g.DrawEllipse(pen, handlePos.X - _handleRadius, handlePos.Y - _handleRadius, _handleRadius * 2f, _handleRadius * 2f);

            g.Restore(gs);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public override bool HandleMouseEvent(object sender, MouseEventArgs mea, V2 mouseScenePos)
        {
            _hot = false;

            PointF screenPos, handlePos;
            double thetaOffset;
            if (GetPositions(out screenPos, out handlePos, out thetaOffset))
            {
                if (Misc.ArePointsCloser(mea.Location, handlePos, _handleRadius))
                    _hot = true;
            }

            if ((mea.Button & MouseButtons.Left) != 0)
            {
                if (_dragging)
                {
                    float dx = (float)mea.Location.X - screenPos.X;
                    float dy = (float)mea.Location.Y - screenPos.Y;

                    Attr.SetValue(Math.Atan2(dy, dx) - thetaOffset);
                }
                else
                {
                    if (_hot)
                    {
                        _dragging = true;
                    }
                }
            }
            else
                _dragging = false;

            return false;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}
