using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PyDoodle
{
    public partial class TweakV2Control : TweakControl
    {
        private Color _defaultBackColour;
        private Color _errorBackColour;

        public TweakV2Control()
        {
            InitializeComponent();

            _defaultBackColour = _xTextBox.BackColor;

            double h, s, v;
            Misc.ColorToHSV(_defaultBackColour, out h, out s, out v);
            h = 0;//red
            s = 0.25;
            _errorBackColour = Misc.ColorFromHSV(h, s, v);
        }

        public override void SetValue(dynamic newValue)
        {
            if (newValue is V2)
            {
                V2 v2 = (V2)newValue;

                _xTextBox.Text = v2.x.ToString();
                _yTextBox.Text = v2.y.ToString();

                _xTextBox.BackColor = _defaultBackColour;
                _yTextBox.BackColor = _defaultBackColour;
            }
        }

        private void _xTextBox_TextChanged(object sender, System.EventArgs e)
        {
            _xTextBox.BackColor = Control.DefaultBackColor;
        }

        private void _yTextBox_TextChanged(object sender, System.EventArgs e)
        {
            _yTextBox.BackColor = Control.DefaultBackColor;
        }

        // http://www.codeproject.com/Articles/36648/Default-Button-on-a-User-Control
        protected override bool ProcessKeyPreview(ref Message m)
        {
            if (m.Msg == 0x100)//WM_KEYDOWN
            {
                if (m.WParam.ToInt32() == (int)ConsoleKey.Enter)
                {
                    if (this.Tweak != null)
                    {
                        double x, y;

                        bool goodX = double.TryParse(_xTextBox.Text, out x);
                        bool goodY = double.TryParse(_yTextBox.Text, out y);

                        if (goodX && goodY)
                        {
                            this.Tweak(this, new EventArgs(new V2(x, y)));

                            _xTextBox.BackColor = _defaultBackColour;
                            _yTextBox.BackColor = _defaultBackColour;
                        }
                        else
                        {
                            if (!goodX)
                                _xTextBox.BackColor = _errorBackColour;

                            if (!goodY)
                                _yTextBox.BackColor = _errorBackColour;
                        }
                    }
                }
            }

            return base.ProcessKeyPreview(ref m);
        }
    }
}
