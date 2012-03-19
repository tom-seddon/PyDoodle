using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PyDoodle
{
    public class Main
    {
        public class State
        {
            public List<String> RecentFiles = new List<string>();
        }

        private State _state;

        private static readonly string stateFileName = "PyDoodle.config.xml";

        public EventHandler RecentFileListChanged;

        public IEnumerable<string> RecentFileList
        {
            get { return _state.RecentFiles; }
        }

        public Main()
        {
            _state = Misc.LoadXmlOrCreateDefault<State>(stateFileName);
        }

        public void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(this));

            Misc.SaveXml(stateFileName, _state);
        }

        public void OnRecentFileUsed(string fileName)
        {
            for (int i = 0; i < _state.RecentFiles.Count; ++i)
            {
                if (Misc.AreFileNamesEqual(fileName, _state.RecentFiles[i]))
                {
                    _state.RecentFiles.RemoveAt(i);
                    break;
                }
            }

            _state.RecentFiles.Insert(0, fileName);

            while (_state.RecentFiles.Count > 10)
                _state.RecentFiles.RemoveAt(_state.RecentFiles.Count - 1);

            OnRecentFileListChanged();
        }

        protected void OnRecentFileListChanged()
        {
            if (RecentFileListChanged != null)
                RecentFileListChanged(this, EventArgs.Empty);
        }
    }

    public class Program
    {
        public class GlobalConfig
        {
            public List<string> recentFiles = new List<string>();
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Main m = new Main();
            m.Run();
        }
    }
}
