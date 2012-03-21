using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace PyDoodle
{
    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////

    class FloatMouseDragHandler
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private float _scale;
        private Point _lastLocation;
        private TextBox _dragTextBox;
        private Cursor _oldDragTextBoxCursor;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        public EventHandler TextBoxChanged;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        public FloatMouseDragHandler(float scale)
        {
            _scale = scale;

            _dragTextBox = null;
            _oldDragTextBoxCursor = null;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Middle) != 0)
            {
                if (_dragTextBox == null)
                {
                    _dragTextBox = sender as TextBox;
                    _lastLocation = e.Location;

                    if (_dragTextBox != null)
                    {
                        _oldDragTextBoxCursor = _dragTextBox.Cursor;
                        _dragTextBox.Cursor = Cursors.VSplit;
                    }
                }

                if (_dragTextBox != null)
                {
                    double value;
                    if (double.TryParse(_dragTextBox.Text, out value))
                    {
                        value += (e.Location.X - _lastLocation.X) * Scale;

                        _dragTextBox.Text = string.Format("{0:N4}", value);

                        OnTextBoxChanged(_dragTextBox);
                    }
                }

                _lastLocation = e.Location;
            }
            else
            {
                if (_dragTextBox != null)
                {
                    _dragTextBox.Cursor = _oldDragTextBoxCursor;

                    _dragTextBox = null;
                }
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
        
        protected void OnTextBoxChanged(object sender)
        {
            if (TextBoxChanged != null)
                TextBoxChanged(sender, EventArgs.Empty);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}
