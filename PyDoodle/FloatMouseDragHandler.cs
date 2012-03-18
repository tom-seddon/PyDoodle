using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace PyDoodle
{
    class FloatMouseDragHandler
    {
        private float _scale;
        private bool _isDragging = false;
        private Point _lastLocation;

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public EventHandler TextBoxChanged;

        public FloatMouseDragHandler(float scale)
        {
            _scale = scale;
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Middle) != 0)
            {
                if (!_isDragging)
                {
                    _isDragging = true;
                    _lastLocation = e.Location;
                }

                TextBox textBox = sender as TextBox;
                if (textBox != null)
                {
                    double value;
                    if (double.TryParse(textBox.Text, out value))
                    {
                        value += (e.Location.X - _lastLocation.X) * Scale;

                        textBox.Text = string.Format("{0:N4}", value);

                        OnTextBoxChanged(textBox);
                    }
                }

                if (_isDragging)
                    _lastLocation = e.Location;
            }
            else
                _isDragging = false;
        }

        protected void OnTextBoxChanged(object sender)
        {
            if (TextBoxChanged != null)
                TextBoxChanged(sender, EventArgs.Empty);
        }
    }
}
