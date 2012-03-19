using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Drawing.Drawing2D;

namespace PyDoodle
{
    public partial class GraphicsPanel : DockContent
    {
        public GraphicsPanel()
        {
            InitializeComponent();

            _showGridCheckBox.Checked = _graphicsControl.ShowGrid;
            _yIsUp.Checked = _graphicsControl.YIsUp;

            ResetGraphicsTransform();
        }

        public GraphicsControl GraphicsControl
        {
            get { return _graphicsControl; }
        }

        private void _showGridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _graphicsControl.ShowGrid = _showGridCheckBox.Checked;
        }

        private void _resetButton_Click(object sender, EventArgs e)
        {
            ResetGraphicsTransform();
        }

        private void ResetGraphicsTransform()
        {
            GraphicsTransform = new Matrix(1f, 0f, 0f, 1f, this.Width * .5f, this.Height * .5f);
        }

        public Matrix GraphicsTransform
        {
            get { return _graphicsControl.Transform.Clone(); }
            set { _graphicsControl.Transform = value; }
        }

        public bool GraphicsShowGrid
        {
            get { return _showGridCheckBox.Checked; }
            set { _showGridCheckBox.Checked = value; }
        }

        public bool GraphicsYIsUp
        {
            get { return _yIsUp.Checked; }
            set { _yIsUp.Checked = value; }
        }

        private void _yIsUp_CheckedChanged(object sender, EventArgs e)
        {
            _graphicsControl.YIsUp = _yIsUp.Checked;
        }
    }
}
