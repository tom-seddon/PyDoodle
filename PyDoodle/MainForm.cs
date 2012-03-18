using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;

namespace PyDoodle
{
    public partial class MainForm : Form
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private GraphicsPanel _graphicsPanel;
        private TextPanel _textPanel;
        private TweaksPanel _tweaksPanel;

        private ScriptEngine _scriptEngine;
        private ObjectOperations _objectOperations;
        private pydoodleModule _pydoodleModule;
        private ScriptScope _scriptScope;

        private string _scriptFileName;
        private bool _isRerunPending;
        private object _runPyobj;

        private FileChangeWatcher _fileChangeWatcher;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public object RunPyobj
        {
            get { return _runPyobj; }
            set { _runPyobj = value; }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        // bleargh
        public TweaksPanel TweaksPanel
        {
            get { return _tweaksPanel; }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public MainForm()
        {
            InitializeComponent();

            _graphicsPanel = new GraphicsPanel();
            _graphicsPanel.Show(_dockPanel, DockState.Document);
            _graphicsPanel.GraphicsControl.DelegatedPaint += this.OnGraphicsPanelPaint;

            _textPanel = new TextPanel();
            _textPanel.Show(_dockPanel, DockState.Document);

            _tweaksPanel = new TweaksPanel();
            _tweaksPanel.Show(_dockPanel, DockState.Document);

            _graphicsPanel.Show(_dockPanel);

            _scriptEngine = null;
            _objectOperations = null;
            _pydoodleModule = null;
            _scriptScope = null;

            RunScript(@"C:\tom\pydoodle\test.py");
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void OnGraphicsPanelPaint(object sender, PaintEventArgs e)
        {
            if (_runPyobj != null)
            {
                _pydoodleModule.Graphics = e.Graphics;

                _objectOperations.InvokeMember(_runPyobj, "tick");

                _pydoodleModule.Graphics = null;
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

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

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleFileChangeWatcherChanged(object sender, FileSystemEventArgs fsea)
        {
            if (!_isRerunPending)
            {
                this.BeginInvoke(new Action<string>(this.RunScript), new object[] { _scriptFileName, });
                _isRerunPending = true;
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void StopScript()
        {
            if (_fileChangeWatcher != null)
            {
                _fileChangeWatcher.Changed -= this.HandleFileChangeWatcherChanged;

                _fileChangeWatcher.Dispose();
                _fileChangeWatcher = null;
            }

            _scriptEngine = null;
            _objectOperations = null;
            _pydoodleModule = null;
            _scriptScope = null;
            _scriptFileName = null;
            _isRerunPending = false;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void RunScript(string fileName)
        {
            StopScript();

            // Initialise basic script stuff.
            Dictionary<string, object> options = new Dictionary<string, object>();
            options["Debug"] = true;

            _scriptEngine = Python.CreateEngine(options);

            _objectOperations = _scriptEngine.CreateOperations();

            // Add pydoodle module.
            _pydoodleModule = new pydoodleModule(_scriptEngine, this);

            // Link with tweaks panel.
            _tweaksPanel.Reset(_scriptEngine);

            // Fiddle with python module search paths. Replace "." with the path to the loaded script.
            string filePath = Misc.GetPathDirectoryName(fileName);
            if (filePath != null)
            {
                List<string> paths = new List<string>(_scriptEngine.GetSearchPaths());

                paths.Remove(".");
                paths.Insert(0, filePath);

                _scriptEngine.SetSearchPaths(paths);
            }

            foreach (string path in _scriptEngine.GetSearchPaths())
                Console.WriteLine("Search Path: {0}", path);

            // go!
            _scriptFileName = fileName;
            _scriptScope = _scriptEngine.ExecuteFile(_scriptFileName);

            // Watch for further changes.
            _fileChangeWatcher = new FileChangeWatcher(Misc.GetModuleFileNames(_scriptEngine));
            _fileChangeWatcher.Changed += this.HandleFileChangeWatcherChanged;
            _isRerunPending = false;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_runPyobj == null)
                return;

            _graphicsPanel.GraphicsControl.Invalidate();
        }
    }
}
