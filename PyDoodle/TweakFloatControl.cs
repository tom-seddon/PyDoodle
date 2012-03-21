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
    public partial class TweakFloatControl : TweakControl
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        bool _isFloat;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public TweakFloatControl(Attr attr = null)
            : base(attr)
        {
            InitializeComponent();

            _isFloat = false;

            FloatMouseDragHandler fmdh = new FloatMouseDragHandler(.1f);

            fmdh.TextBoxChanged += this.HandleScriptValueDirty;

            _textBox.MouseMove += fmdh.HandleMouseMove;

            _textBox.TextChanged += this.HandleTextBoxTextChanged;

            EnterPressed += this.HandleScriptValueDirty;

            AddAttrValueSetHandler(HandleAttrValueSet);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleScriptValueDirty(object sender, System.EventArgs e)
        {
            double value;
            if (double.TryParse(_textBox.Text, out value))
            {
                Attr.SilentSetValue(_isFloat ? (float)value : value);

                Misc.SetCleanColour(_textBox);
            }
            else
                Misc.SetBadColour(_textBox);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleTextBoxTextChanged(object sender, System.EventArgs e)
        {
            Misc.SetDirtyColour(_textBox);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleAttrValueSet(object sender, EventArgs e)
        {
            object newValue = ((Attr)sender).GetValue();

            if (newValue is float || newValue is double)
            {
                _isFloat = newValue is float;

                _textBox.Text = newValue.ToString();

                Misc.SetCleanColour(_textBox);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }
}
