using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PyDoodle
{
    public partial class TextPanel : DockContent
    {
        public TextPanel()
        {
            InitializeComponent();
        }

        public string PanelText
        {
            get { return _textBox.Text; }
            set { _textBox.Text = value; }
        }
    }
}
