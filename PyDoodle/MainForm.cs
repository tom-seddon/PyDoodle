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

        private ScriptEngine _scriptEngine;
        private List<string> _scriptEngineDefaultSearchPaths;
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

        public MainForm()
        {
            InitializeComponent();

            Dictionary<string, object> options = new Dictionary<string, object>();

            options["Debug"] = true;

            _scriptEngine = Python.CreateEngine(options);
            _scriptEngineDefaultSearchPaths = new List<string>(_scriptEngine.GetSearchPaths());

            _objectOperations = _scriptEngine.CreateOperations();

            _graphicsPanel = new GraphicsPanel();
            _graphicsPanel.Show(_dockPanel, DockState.Document);
            _graphicsPanel.GraphicsControl.DelegatedPaint += this.OnGraphicsPanelPaint;

            _textPanel = new TextPanel();
            _textPanel.Show(_dockPanel, DockState.Document);

            _tweaksPanel = new TweaksPanel(_scriptEngine);
            _tweaksPanel.Show(_dockPanel, DockState.Document);

            _graphicsPanel.Show(_dockPanel);

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
            string filePath = Misc.TryGetDirectoryName(fileName);
            if (filePath != null)
            {
                // Replace "." in the search paths with the path to the file.
                List<string> paths = new List<string>(_scriptEngineDefaultSearchPaths);

                paths.Remove(".");
                paths.Insert(0, filePath);

                _scriptEngine.SetSearchPaths(paths);
            }

            foreach (string path in _scriptEngine.GetSearchPaths())
                Console.WriteLine("Search Path: {0}", path);

            _scriptScope = _scriptEngine.ExecuteFile(fileName);

            foreach (string module in GetModuleFileNames(_scriptEngine))
                Console.WriteLine("Module: {0}", module);
        }

        private static List<string> GetModuleFileNames(ScriptEngine se)
        {
            List<string> moduleFileNames = new List<string>();

            PythonDictionary modules = se.GetSysModule().GetVariable("modules") as PythonDictionary;
            if (modules != null)
            {
                foreach (KeyValuePair<object, object> kvp in modules)
                {
                    PythonModule module = kvp.Value as PythonModule;

                    if (module != null)
                    {
                        object fileNameObj;
                        if (module.Get__dict__().TryGetValue("__file__", out fileNameObj))
                        {
                            string fileName = fileNameObj as string;
                            if (fileName != null && fileName.Length > 0)
                                moduleFileNames.Add((string)fileName);
                        }
                    }
                }
            }

            return moduleFileNames;
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
