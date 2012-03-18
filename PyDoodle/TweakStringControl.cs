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
        public class EventArgs :
            System.EventArgs
        {
            private string _text;

            internal EventArgs(string text)
            {
                _text = text;
            }

            public string Text
            {
                get { return _text; }
            }
        }

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
