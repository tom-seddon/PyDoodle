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
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private bool _showGrid;
        private bool _yIsUp;

        private Pen _posXPen, _negXPen, _posYPen, _negYPen;

        private Matrix _matrix;

        enum DragState
        {
            None,
            Translate,
            Rotate,
            Handle,
        }

        private DragState _dragState;

        private PointF _dragControlPos;
        private V2 _dragScenePos;
        private Handle _dragHandle;

        public PaintEventHandler DelegatedPaint;

        private List<Handle> _handles;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

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

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public bool YIsUp
        {
            get { return _yIsUp; }
            set
            {
                if (_yIsUp != value)
                {
                    _yIsUp = value;
                    this.Invalidate();
                }
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public GraphicsControl()
        {
            InitializeComponent();

            _showGrid = true;
            _yIsUp = false;

            _posXPen = new Pen(Color.Red, 0);
            _negXPen = new Pen(Color.DarkRed, 0);

            _posYPen = new Pen(Color.Green, 0);
            _negYPen = new Pen(Color.DarkGreen, 0);

            _matrix = new Matrix();

            _dragState = DragState.None;
            _dragHandle = null;

            this.Paint += this.HandlePaint;
            this.MouseMove += this.HandleMouseMove;
            this.MouseWheel += this.HandleMouseWheel;

            _handles = new List<Handle>();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void ResetHandlesList()
        {
            _handles.Clear();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void AddHandle(Handle handle)
        {
            _handles.Add(handle);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandlePaint(object sender, PaintEventArgs pea)
        {
            Matrix m = new Matrix();

            {
                float[] e = Transform.Elements;

                if (_yIsUp)
                {
                    e[2] = -e[2];
                    e[3] = -e[3];
                }

                pea.Graphics.Transform = new Matrix(e[0], e[1], e[2], e[3], e[4], e[5]);
            }

            pea.Graphics.Clear(Color.White);

            if (_showGrid)
            {
                int infinity = 1000;// ...close enough

                pea.Graphics.DrawLine(_posXPen, 0, 0, infinity, 0);
                pea.Graphics.DrawLine(_negXPen, 0, 0, -infinity, 0);
                pea.Graphics.DrawLine(_posYPen, 0, 0, 0, infinity);
                pea.Graphics.DrawLine(_negYPen, 0, 0, 0, -infinity);
            }

            if (this.DelegatedPaint != null)
                this.DelegatedPaint(sender, pea);

            foreach (Handle handle in _handles)
                handle.Draw(pea.Graphics);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private V2 GetScenePosForLocation(Point location)
        {
            PointF[] locationArray = { location };

            Matrix invTransform = _matrix.Clone();
            invTransform.Invert();

            invTransform.TransformPoints(locationArray);

            return new V2(locationArray[0].X, locationArray[0].Y);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private PointF GetLocationForScenePos(V2 scenePos)
        {
            PointF[] scenePosArray = { new PointF((float)scenePos.x, (float)scenePos.y), };
            Transform.TransformPoints(scenePosArray);

            return scenePosArray[0];
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            //_mouseState = new MouseState(e.Location, GetScenePosForLocation(e.Location), e.Button);

            switch (_dragState)
            {
            case DragState.None:
                {
                    switch (e.Button)
                    {
                    case MouseButtons.None:
                        {
                            V2 scenePos = GetScenePosForLocation(e.Location);

                            foreach (Handle handle in _handles)
                                handle.HandleMouseEvent(sender,e,scenePos);
                        }
                        break;

                    case MouseButtons.Left:
                        {
                            V2 scenePos = GetScenePosForLocation(e.Location);

                            foreach (Handle handle in _handles)
                            {
                                if (handle.HandleMouseEvent(sender, e, scenePos))
                                {
                                    _dragState = DragState.Handle;
                                    _dragHandle = handle;
                                    break;
                                }
                            }
                        }
                        break;

                    case MouseButtons.Middle:
                        {
                            _dragControlPos = e.Location;
                            _dragScenePos = GetScenePosForLocation(e.Location);

                            if ((Control.ModifierKeys & Keys.Alt) != 0)
                                _dragState = DragState.Translate;
                            else if ((Control.ModifierKeys & Keys.Control) != 0)
                                _dragState = DragState.Rotate;
                        }
                        break;
                    }
                }
                break;

            case DragState.Rotate:
                {
                    if ((e.Button & MouseButtons.Middle) != 0)
                    {
                        float theta = (float)((e.Location.X - _dragControlPos.X) / 10.0 * Math.PI);

                        _matrix.RotateAt(theta, _dragScenePos.AsPointF());

                        _dragControlPos = e.Location;

                        Invalidate();
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

                        Invalidate();
                    }
                    else
                        _dragState = DragState.None;
                }
                break;

            case DragState.Handle:
                {
                    _dragHandle.HandleMouseEvent(sender, e, GetScenePosForLocation(e.Location));

                    if ((e.Button & MouseButtons.Left) == 0)
                    {
                        _dragHandle = null;

                        _dragState = DragState.None;
                    }
                }
                break;
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

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

            Invalidate();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public Matrix Transform
        {
            get { return _matrix; }
            set { _matrix = value.Clone(); }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}
