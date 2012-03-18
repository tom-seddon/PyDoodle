using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using IronPython.Runtime;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using WeifenLuo.WinFormsUI.Docking;

namespace PyDoodle
{
    public partial class MainForm : Form
    {
        private GraphicsPanel _graphicsPanel;
        private TextPanel _textPanel;
        private TweaksPanel _tweaksPanel;

        private readonly ScriptEngine _scriptEngine;
        private ObjectOperations _objectOperations;
        private object _runPyobj;
        private pydoodleModule _pydoodleModule;
        private ScriptScope _scriptScope;

        public object RunPyobj
        {
            get { return _runPyobj; }
            set { _runPyobj = value; }
        }

        // bleargh
        public TweaksPanel TweaksPanel
        {
            get { return _tweaksPanel; }
        }

        public MainForm(ScriptEngine scriptEngine)
        {
            InitializeComponent();

            _graphicsPanel = new GraphicsPanel();
            _graphicsPanel.Show(_dockPanel, DockState.Document);
            _graphicsPanel.GraphicsControl.DelegatedPaint += this.OnGraphicsPanelPaint;

            _textPanel = new TextPanel();
            _textPanel.Show(_dockPanel, DockState.Document);

            _tweaksPanel = new TweaksPanel(scriptEngine);
            _tweaksPanel.Show(_dockPanel, DockState.Document);

            _graphicsPanel.Show(_dockPanel);

            _scriptEngine = scriptEngine;
            _objectOperations = _scriptEngine.CreateOperations();

            _pydoodleModule = new pydoodleModule(_scriptEngine, this);

            _scriptScope = null;

            ResetScriptStuff();

            RunScript(@"C:\tom\PyDoodle\test.py");
        }

        public void OnGraphicsPanelPaint(object sender, PaintEventArgs e)
        {
            if (_runPyobj != null)
            {
                _pydoodleModule.Graphics = e.Graphics;

                _objectOperations.InvokeMember(_runPyobj, "tick");

                _pydoodleModule.Graphics = null;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.CheckFileExists = true;
            ofd.Title = "Open script...";
            ofd.Filter = "Python file (*.py)|*.py|All files (*.*)|*.*";
            ofd.FilterIndex = 0;

            DialogResult result = ofd.ShowDialog(this);
            if (result != DialogResult.OK)
                return;

            RunScript(ofd.FileName);
        }

        public void RunScript(string fileName)
        {
            _scriptScope = _scriptEngine.ExecuteFile(fileName);

            string[] modules = Python.GetModuleFilenames(_scriptEngine);
            foreach (string module in modules)
                Console.WriteLine("Module: {0}", module);
        }

        private void ResetScriptStuff()
        {
            _runPyobj = null;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_runPyobj == null)
                return;

            _graphicsPanel.GraphicsControl.Invalidate();
        }
    }
}
