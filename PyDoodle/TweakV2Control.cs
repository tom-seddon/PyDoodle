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
        public TweakV2Control()
        {
            InitializeComponent();
        }

        public override void SetValue(dynamic newValue)
        {
            if (newValue is V2)
            {
                V2 v2 = (V2)newValue;

                _xTextBox.Text = v2.x.ToString();
                _yTextBox.Text = v2.y.ToString();
            }
        }

        private void _xTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void _yTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
