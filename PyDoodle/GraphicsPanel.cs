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
    public partial class GraphicsPanel : DockContent
    {
        public GraphicsPanel()
        {
            InitializeComponent();
        }

        public GraphicsControl GraphicsControl
        {
            get { return _graphicsControl; }
        }
    }
}
