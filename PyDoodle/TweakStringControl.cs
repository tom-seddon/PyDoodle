﻿using System;
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
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public TweakStringControl()
        {
            InitializeComponent();

            _textBox.TextChanged += HandleTextBoxDirty;

            EnterPressed += HandleScriptValueDirty;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleTextBoxDirty(object sender, System.EventArgs e)
        {
            Misc.SetDirtyColour(_textBox);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleScriptValueDirty(object sender, System.EventArgs e)
        {
            OnTweak(new EventArgs(_textBox.Text));

            Misc.SetCleanColour(_textBox);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public override void SetValue(dynamic newValue)
        {
            if (newValue is string)
            {
                _textBox.Text = (string)newValue;

                Misc.SetCleanColour(_textBox);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}
