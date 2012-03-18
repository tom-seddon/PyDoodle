using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace PyDoodle
{
    public partial class GraphicsControl : UserControl
    {
        private bool _showGrid;
        private Pen _posXPen, _negXPen, _posYPen, _negYPen;

        private Matrix _matrix;

        enum DragState
        {
            None,
            Translate,
            Rotate,
        }

        private DragState _dragState;

        private PointF _dragControlPos;
        private PointF _dragScenePos;

        public PaintEventHandler DelegatedPaint;

        public bool ShowGrid
        {
            get { return _showGrid; }
            set
            {
                if (_showGrid != value)
                {
                    _showGrid = value;
                    this.Invalidate();
                }
            }
        }

        public GraphicsControl()
        {
            InitializeComponent();

            _showGrid = true;

            _posXPen = new Pen(Color.Red);
            _negXPen = new Pen(Color.DarkRed);

            _posYPen = new Pen(Color.Green);
            _negYPen = new Pen(Color.DarkGreen);

            _matrix = new Matrix();

            _dragState = DragState.None;

            this.Paint += this.HandlePaint;
            this.MouseMove += this.HandleMouseMove;
            this.MouseWheel += this.HandleMouseWheel;
        }

        private void HandlePaint(object sender, PaintEventArgs e)
        {
            Matrix m = new Matrix();

            e.Graphics.Transform = _matrix.Clone();

            e.Graphics.Clear(Color.White);

            if (_showGrid)
            {
                int infinity = 1000;// ...close enough

                e.Graphics.DrawLine(_posXPen, 0, 0, infinity, 0);
                e.Graphics.DrawLine(_negXPen, 0, 0, -infinity, 0);
                e.Graphics.DrawLine(_posYPen, 0, 0, 0, infinity);
                e.Graphics.DrawLine(_negYPen, 0, 0, 0, -infinity);
            }

            if (this.DelegatedPaint != null)
                this.DelegatedPaint(sender, e);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            switch (_dragState)
            {
            case DragState.None:
                {
                    if ((e.Button & MouseButtons.Middle) != 0)
                    {
                        _dragControlPos = e.Location;

                        {
                            Matrix invMatrix = _matrix.Clone();
                            invMatrix.Invert();

                            PointF[] dragScenePos = { e.Location };
                            invMatrix.TransformPoints(dragScenePos);

                            _dragScenePos = dragScenePos[0];
                        }

                        if ((Control.ModifierKeys & Keys.Alt) != 0)
                            _dragState = DragState.Translate;
                        else if ((Control.ModifierKeys & Keys.Control) != 0)
                            _dragState = DragState.Rotate;
                    }
                }
                break;

            case DragState.Rotate:
                {
                    if ((e.Button & MouseButtons.Middle) != 0)
                    {
                        float theta = (float)((e.Location.X - _dragControlPos.X) / 10.0 * Math.PI);

                        _matrix.RotateAt(theta, _dragScenePos);

                        _dragControlPos = e.Location;
                    }
                    else
                        _dragState = DragState.None;
                }
                break;

            case DragState.Translate:
                {
                    if ((e.Button & MouseButtons.Middle) != 0)
                    {
                        _matrix.Translate(e.Location.X - _dragControlPos.X, e.Location.Y - _dragControlPos.Y, MatrixOrder.Append);

                        _dragControlPos = e.Location;
                    }
                    else
                        _dragState = DragState.None;
                }
                break;
            }
        }

        private void HandleMouseWheel(object sender, MouseEventArgs mea)
        {
            float[] e = _matrix.Elements;

            float xmag = (float)Math.Sqrt(e[0] * e[0] + e[1] * e[1]);
            float ymag = (float)Math.Sqrt(e[2] * e[2] + e[3] * e[3]);

            e[0] /= xmag;
            e[1] /= xmag;
            e[2] /= ymag;
            e[3] /= ymag;

            float delta = mea.Delta / 120.0f / 10.0f;

            xmag += delta;
            xmag = Math.Max(xmag, 0.1f);

            ymag = xmag;// uniform scales only for now.

            e[0] *= xmag;
            e[1] *= xmag;
            e[2] *= ymag;
            e[3] *= ymag;

            _matrix = new Matrix(e[0], e[1], e[2], e[3], e[4], e[5]);
        }
    }
}
