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
    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////

    public partial class TweakV2Control : TweakControl
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public TweakV2Control()
        {
            InitializeComponent();

            _xTextBox.TextChanged += HandleTextBoxTextChanged;
            _yTextBox.TextChanged += HandleTextBoxTextChanged;

            FloatMouseDragHandler fmdh = new FloatMouseDragHandler(.1f);
            fmdh.TextBoxChanged += HandleScriptValueDirty;

            _xTextBox.MouseMove += fmdh.HandleMouseMove;
            _yTextBox.MouseMove += fmdh.HandleMouseMove;

            EnterPressed += HandleScriptValueDirty;

        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public override void SetValue(dynamic newValue)
        {
            if (newValue is V2)
            {
                V2 v2 = (V2)newValue;

                _xTextBox.Text = v2.x.ToString();
                _yTextBox.Text = v2.y.ToString();

                Misc.SetCleanColour(_xTextBox, _yTextBox);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleTextBoxTextChanged(object sender, System.EventArgs e)
        {
            Misc.SetDirtyColour((TextBox)sender);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleScriptValueDirty(object sender, System.EventArgs e)
        {
            double x, y;

            bool goodX = double.TryParse(_xTextBox.Text, out x);
            bool goodY = double.TryParse(_yTextBox.Text, out y);

            if (goodX && goodY)
            {
                OnTweak(new EventArgs(new V2(x, y)));

                Misc.SetCleanColour(_xTextBox, _yTextBox);
            }
            else
                Misc.SetBadColour(goodX ? null : _xTextBox, goodY ? null : _yTextBox);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }

    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////
}
