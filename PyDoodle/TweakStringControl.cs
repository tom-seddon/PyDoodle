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
    public partial class TweakStringControl : TweakControl
    {
        public TweakStringControl()
        {
            InitializeComponent();
        }

        public override void SetValue(dynamic newValue)
        {
            if (newValue is string)
                _textBox.Text = (string)newValue;
        }
    }
}
