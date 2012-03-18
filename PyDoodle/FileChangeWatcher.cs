using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace PyDoodle
{
    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////

    class FileChangeWatcher : IDisposable
    {
        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private class DirWatcher : IDisposable
        {
            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            private HashSet<string> _fileNames;
            private FileSystemWatcher _watcher;
            private FileChangeWatcher _owner;

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public DirWatcher(FileChangeWatcher owner, string dir)
            {
                _fileNames = new HashSet<string>();

                _watcher = new FileSystemWatcher(dir);
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
                _watcher.Changed += this.HandleChanged;

                _owner = owner;
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public void Dispose()
            {
                _watcher.Dispose();
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public void AddFileName(string fileName)
            {
                _fileNames.Add(fileName.ToLower());
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public void HandleChanged(object sender, FileSystemEventArgs fsea)
            {
                if (_fileNames.Contains(fsea.Name.ToLower()))
                {
                    //Console.WriteLine("FileChangeWatcher.HandleChanged: ChangeType={0} Name=\"{1}\"", fsea.ChangeType, fsea.Name);
                    if (_owner.Changed != null)
                        _owner.Changed(_owner, fsea);
                }
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////

            public void BeginWatching()
            {
                _watcher.EnableRaisingEvents = true;
            }

            //-///////////////////////////////////////////////////////////////////////
            //-///////////////////////////////////////////////////////////////////////
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        private List<DirWatcher> _dirWatchers;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public FileSystemEventHandler Changed;

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialise new FileChangeWatcher that will watch the given files.
        /// </summary>
        /// <param name="fileNames">
        /// names of files to watch. Each file will be watched only the once, no
        /// matter how many times it might be mentioned.</param>
        public FileChangeWatcher(ICollection<string> fileNames)
        {
            var dirWatchersByDirName = new Dictionary<string, DirWatcher>();
            _dirWatchers = new List<DirWatcher>();

            foreach (string fileName in fileNames)
            {
                var dir = Misc.GetPathDirectoryName(fileName);
                var name = Misc.GetPathFileName(fileName);

                if (dir == null || name == null)
                    continue;

                DirWatcher dirWatcher;

                if (!dirWatchersByDirName.TryGetValue(dir, out dirWatcher))
                {
                    dirWatcher = new DirWatcher(this, dir);

                    dirWatchersByDirName.Add(dir, dirWatcher);
                    _dirWatchers.Add(dirWatcher);
                }

                dirWatcher.AddFileName(name);
            }

            foreach (DirWatcher dirWatcher in _dirWatchers)
                dirWatcher.BeginWatching();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            foreach (DirWatcher dirWatcher in _dirWatchers)
                dirWatcher.Dispose();
        }

        //-///////////////////////////////////////////////////////////////////////
        //-///////////////////////////////////////////////////////////////////////
    }

    //-///////////////////////////////////////////////////////////////////////
    //-///////////////////////////////////////////////////////////////////////
}
