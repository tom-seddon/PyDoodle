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
using System.Drawing.Drawing2D;
using System.Runtime.Remoting;
using Microsoft.Scripting.Runtime;

namespace PyDoodle
{
    public partial class MainForm : Form
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public class Config
        {
            // Graphics panel settings.
            public float[] GraphicsTransformElements = null;
            public bool GraphicsShowGrid = true;
            public bool GraphicsYIsUp = false;
            // 

            // Placement of this form.
            public WindowPlacement WindowPlacement = null;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private Main _main;

        private GraphicsPanel _graphicsPanel;
        private TextPanel _textPanel;
        private TweaksPanel _tweaksPanel;

        private ScriptEngine _scriptEngine;
        private ObjectOperations _objectOperations;
        private pydoodleModule _pydoodleModule;
        private ScriptScope _scriptScope;
        private ScriptSource _scriptSource;
        private Exception _scriptException;

        enum ScriptState
        {
            NoneLoaded,
            RunGlobal,
            RunTick,
            RerunPending,
            Borked,
        }

        private string _scriptFileName;
        private ScriptState _scriptState;
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

        public GraphicsPanel GraphicsPanel
        {
            get { return _graphicsPanel; }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void FindDockContent<T>(out T v) where T : DockContent
        {
            v = null;

            foreach (object o in _dockPanel.Contents)
            {
                T to = o as T;

                if (to != null)
                {
                    v = to;
                    break;
                }
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void PostLoadPanels()
        {
            // Find interesting panels.
            FindDockContent(out _textPanel);
            FindDockContent(out _tweaksPanel);
            FindDockContent(out _graphicsPanel);

            //
            if (_textPanel == null)
            {
                _textPanel = new TextPanel();
                _textPanel.Show(_dockPanel, DockState.Document);
            }

            //
            if (_tweaksPanel == null)
            {
                _tweaksPanel = new TweaksPanel();
                _tweaksPanel.Show(_dockPanel, DockState.Document);
            }

            //
            if (_graphicsPanel == null)
            {
                _graphicsPanel = new GraphicsPanel();
                _graphicsPanel.Show(_dockPanel, DockState.Document);
            }

            _graphicsPanel.GraphicsControl.DelegatedPaint += this.OnGraphicsPanelPaint;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public MainForm(Main main)
        {
            InitializeComponent();

            _main = main;
            _main.RecentFileListChanged += this.HandleRebuildRecentFilesList;
            HandleRebuildRecentFilesList(null, null);

            _graphicsPanel = null;
            _textPanel = null;
            _tweaksPanel = null;

            PostLoadPanels();

            _fileChangeWatcher = null;

            StopScript();

            if (_main.LastFile != null)
                RunScript(_main.LastFile, true, false);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleRebuildRecentFilesList(object sender, EventArgs ea)
        {
            int preIdx = _fileMenu.DropDownItems.IndexOf(_preMRUSeparator);

            while (!object.ReferenceEquals(_fileMenu.DropDownItems[preIdx + 1], _postMRUSeparator))
                _fileMenu.DropDownItems.RemoveAt(preIdx + 1);

            int insertIdx = preIdx + 1;

            foreach (string recentFile in _main.RecentFileList)
            {
                ToolStripMenuItem tsmi = new ToolStripMenuItem();

                tsmi.Text = recentFile;
                tsmi.Tag = recentFile;
                tsmi.Click += HandleRecentFileClick;

                _fileMenu.DropDownItems.Insert(insertIdx++, tsmi);
            }

            _postMRUSeparator.Visible = insertIdx > preIdx + 1;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleRecentFileClick(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi != null)
            {
                string fileName = tsmi.Tag as string;
                if (fileName != null)
                    RunScript(fileName, true, false);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void OnGraphicsPanelPaint(object sender, PaintEventArgs pea)
        {
            if (_scriptState == ScriptState.Borked)
            {
                // Oops!
                pea.Graphics.Transform = new Matrix();

                pea.Graphics.Clear(Color.DarkRed);

                pea.Graphics.DrawString(_scriptException.ToString(), SystemFonts.MessageBoxFont, new SolidBrush(Color.White), 0f, 0f);
            }
            else
                TickPython(pea.Graphics);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void TickPython(Graphics g)
        {
            if (_scriptEngine == null)
                return;

            _pydoodleModule.BeginDraw(g, GraphicsPanel.GraphicsControl);

            try
            {
                switch (_scriptState)
                {
                case ScriptState.RunGlobal:
                    {
                        _scriptSource.Execute();

                        _scriptState = ScriptState.RunTick;
                    }
                    break;

                case ScriptState.RunTick:
                    {
                        if (_runPyobj != null)
                        {
                            //_textPanel.ClearText();
                            _pydoodleModule.GraphicsControl.ResetHandlesList();

                            _objectOperations.InvokeMember(_runPyobj, "tick");

                            _textPanel.ClearOnNextText = true;
                        }
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                _scriptException = e;
                _scriptState = ScriptState.Borked;

                _textPanel.AppendText("Exception: " + e.Message + Environment.NewLine);
                _textPanel.AppendText(Misc.GetScriptExceptionDynamicStackFramesTrace(e));

                _graphicsPanel.GraphicsControl.ResetHandlesList();
            }

            _pydoodleModule.EndDraw();
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

            RunScript(ofd.FileName, true, false);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void HandleFileChangeWatcherChanged(object sender, FileSystemEventArgs fsea)
        {
            RestartScript(true);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void RestartScript(bool keepValues)
        {
            if (_scriptState != ScriptState.RerunPending)
            {
                this.BeginInvoke(new Action<string, bool, bool>(this.RunScript), new object[] { _scriptFileName, false, keepValues, });

                _scriptState = ScriptState.RerunPending;
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
            _scriptSource = null;
            _scriptException = null;
            _scriptFileName = null;
            _scriptState = ScriptState.NoneLoaded;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void RunScript(string fileName, bool loadLayoutForScript, bool mayKeepValues)
        {
            List<SavedAttrValue> savedAttrValues = null;

            if (mayKeepValues)
            {
                if (_scriptFileName != null)
                {
                    if (Misc.AreFileNamesEqual(fileName, _scriptFileName))
                        savedAttrValues = _pydoodleModule.GetSavedAttrValues();
                }
            }

            SaveStateForScript();

            StopScript();

            //
            if (loadLayoutForScript)
                LoadLayoutForScript(fileName);

            LoadStateForScript(fileName);

            // Initialise basic script stuff.
            Dictionary<string, object> options = new Dictionary<string, object>();
            options["Debug"] = true;

            _scriptEngine = Python.CreateEngine(options);

            Stream tmp = new TextPanel.WriteStream(_textPanel);
            StreamWriter tmpWriter = new StreamWriter(tmp);

            _scriptEngine.Runtime.IO.SetOutput(tmp, tmpWriter);
            _scriptEngine.Runtime.IO.SetErrorOutput(tmp, tmpWriter);

            _objectOperations = _scriptEngine.CreateOperations();

            // Add pydoodle module.
            _pydoodleModule = new pydoodleModule(_scriptEngine, this, savedAttrValues);

            // Reset panels.
            _tweaksPanel.Reset();
            _textPanel.ClearText();

            // Fiddle with python module search paths. Replace "." with the path to the loaded script.
            string filePath = Misc.GetPathDirectoryName(fileName);
            if (filePath != null)
            {
                List<string> paths = new List<string>(_scriptEngine.GetSearchPaths());

                paths.Remove(".");
                paths.Insert(0, filePath);

                _scriptEngine.SetSearchPaths(paths);
            }

            //
            _main.OnRecentFileUsed(fileName);

            // go!
            _scriptFileName = fileName;
            _scriptScope = _scriptEngine.CreateScope();
            _scriptSource = _scriptEngine.CreateScriptSourceFromFile(_scriptFileName);
            _scriptState = ScriptState.RunGlobal;

            TickPython(null);

            List<string> moduleFileNames = Misc.GetModuleFileNames(_scriptEngine);

            // If it's broken already, the module file names list might not
            // include the module that threw the exception. So, try to find that
            // from the exception. Also, always add the script file name.
            if (_scriptState == ScriptState.Borked)
            {
                foreach (DynamicStackFrame stackFrame in Misc.GetScriptExceptionDynamicStackFrames(_scriptException))
                    moduleFileNames.Add(stackFrame.GetFileName());

                moduleFileNames.Add(_scriptFileName);
            }

            // Watch for further changes.
            _fileChangeWatcher = new FileChangeWatcher(moduleFileNames);
            _fileChangeWatcher.Changed += this.HandleFileChangeWatcherChanged;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_runPyobj == null)
                return;

            _graphicsPanel.GraphicsControl.Invalidate();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveStateForScript();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private static string GetLayoutFileName(string scriptFileName)
        {
            return scriptFileName + ".layout.xml";
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private static string GetConfigFileName(string scriptFileName)
        {
            return scriptFileName + ".config.xml";
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void SaveStateForScript()
        {
            if (_scriptFileName == null)
                return;

            // Save layout
            {
                _dockPanel.SaveAsXml(GetLayoutFileName(_scriptFileName), Misc.defaultXmlEncoding);
            }

            // Save config
            {
                Config config = new Config();

                config.GraphicsTransformElements = _graphicsPanel.GraphicsTransform.Elements;
                config.GraphicsShowGrid = _graphicsPanel.GraphicsShowGrid;
                config.GraphicsYIsUp = _graphicsPanel.GraphicsYIsUp;

                config.WindowPlacement = WindowPlacement.GetWindowPlacement(this);

                Misc.SaveXml(GetConfigFileName(_scriptFileName), config);
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private IDockContent HandleDeserializeDockContent(string persistString)
        {
            ObjectHandle oh = Activator.CreateInstance(null, persistString);
            object o = oh.Unwrap();
            IDockContent dockContent = o as IDockContent;
            return dockContent;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void LoadLayoutForScript(string scriptFileName)
        {
            Config config = Misc.LoadXmlOrCreateDefault<Config>(GetConfigFileName(scriptFileName));

            // Set main form placement
            if (config.WindowPlacement != null)
                config.WindowPlacement.Set(this);

            // Load dock layout
            {
                string layoutFileName = GetLayoutFileName(scriptFileName);

                // Annoyingly, a new layout can't be reloaded unless the
                // old one is destroyed, so let's hope that loading the
                // layout in works. This checks that the file exists first,
                // but that's all.
                if (File.Exists(layoutFileName))
                {
                    while (_dockPanel.Contents.Count > 0)
                    {
                        var w = _dockPanel.Contents[0];

                        w.DockHandler.Close();

                        Misc.Dispose(w);
                    }

                    _dockPanel.LoadFromXml(layoutFileName, this.HandleDeserializeDockContent);
                }

                PostLoadPanels();
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void LoadStateForScript(string scriptFileName)
        {
            Config config = Misc.LoadXmlOrCreateDefault<Config>(GetConfigFileName(scriptFileName));

            // Load graphics panel settings
            {
                if (config.GraphicsTransformElements != null && config.GraphicsTransformElements.Length >= 6)
                {
                    _graphicsPanel.GraphicsTransform = new Matrix(
                        config.GraphicsTransformElements[0], config.GraphicsTransformElements[1],
                        config.GraphicsTransformElements[2], config.GraphicsTransformElements[3],
                        config.GraphicsTransformElements[4], config.GraphicsTransformElements[5]);
                }

                _graphicsPanel.GraphicsYIsUp = config.GraphicsYIsUp;
                _graphicsPanel.GraphicsShowGrid = config.GraphicsShowGrid;
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "An interactive programmable plaything", "PyDoodle");
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void yIsUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenuItem(sender);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void showgridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMenuItem(sender);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void resetCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _graphicsPanel.ResetGraphicsTransform();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void ToggleMenuItem(object item)
        {
            ToolStripMenuItem tsmi = (ToolStripMenuItem)item;

            tsmi.Checked = !tsmi.Checked;
            UpdateChecks();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            UpdateChecks();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void UpdateChecks()
        {
            yIsUpToolStripMenuItem.Checked = _graphicsPanel.GraphicsYIsUp;
            showgridToolStripMenuItem.Checked = _graphicsPanel.GraphicsShowGrid;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private void resetScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestartScript(false);
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }

    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////
}
