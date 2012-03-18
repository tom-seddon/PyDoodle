﻿using IronPython.Hosting;
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
            // Transform matrix for the graphics panel.
            public float[] graphicsTransformElements = null;

            // Placement of this form.
            public WindowPlacement windowPlacement = null;
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

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

        public MainForm()
        {
            InitializeComponent();

            _graphicsPanel = null;
            _textPanel = null;
            _tweaksPanel = null;

            PostLoadPanels();

            _fileChangeWatcher = null;

            StopScript();

            LoadStateForScript(@"C:\tom\pydoodle\test.py");
            RunScript(@"C:\tom\pydoodle\test.py");
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
            _pydoodleModule.Graphics = g;

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
                            _objectOperations.InvokeMember(_runPyobj, "tick");
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                _scriptException = e;
                _scriptState = ScriptState.Borked;

                List<object> keys = new List<object>(from object key in _scriptException.Data.Keys select key);
                List<object> values = new List<object>(from object value in _scriptException.Data.Values select value);

                for (int i = 0; i < keys.Count; ++i)
                {
                    Console.WriteLine("{0} ({1}): {2} ({3})", keys[i], keys[i].GetType(), values[i], values[i].GetType());
                }

                Console.WriteLine("meh");
            }

            _pydoodleModule.Graphics = null;
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
            if (_scriptState != ScriptState.RerunPending)
            {
                this.BeginInvoke(new Action<string>(this.RunScript), new object[] { _scriptFileName, });

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
            _scriptScope = _scriptEngine.CreateScope();
            _scriptSource = _scriptEngine.CreateScriptSourceFromFile(_scriptFileName);
            _scriptState = ScriptState.RunGlobal;

            TickPython(null);

            List<string> moduleFileNames = Misc.GetModuleFileNames(_scriptEngine);

            // If it's broken already, the module file names list might not
            // include the module that threw the exception. So, try to find that
            // from the exception.
            //
            // 
            if (_scriptState == ScriptState.Borked)
            {
                Type key = typeof(DynamicStackFrame);
                if (_scriptException.Data.Contains(key))
                {
                    var stackFrames = _scriptException.Data[key] as List<DynamicStackFrame>;
                    if (stackFrames != null)
                    {
                        foreach (DynamicStackFrame stackFrame in stackFrames)
                            moduleFileNames.Add(stackFrame.GetFileName());
                    }
                }
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

                config.graphicsTransformElements = _graphicsPanel.GraphicsTransform.Elements;
                config.windowPlacement = WindowPlacement.GetWindowPlacement(this);

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

        private void LoadStateForScript(string scriptFileName)
        {
            Config config = Misc.LoadXml<Config>(GetConfigFileName(scriptFileName));
            if (config == null)
                config = new Config();

            // Set main form placement
            if (config.windowPlacement != null)
                config.windowPlacement.Set(this);

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

            // Load graphics panel transform
            {
                if (config.graphicsTransformElements != null && config.graphicsTransformElements.Length >= 6)
                {
                    _graphicsPanel.GraphicsTransform = new Matrix(
                        config.graphicsTransformElements[0], config.graphicsTransformElements[1],
                        config.graphicsTransformElements[2], config.graphicsTransformElements[3],
                        config.graphicsTransformElements[4], config.graphicsTransformElements[5]);
                }
            }
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }

    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////
}
