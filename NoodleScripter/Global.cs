using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;
using NoodleScripter.Commands;
using NoodleScripter.Models;
using NoodleScripter.Models.BeatSaber;

namespace NoodleScripter
{
    public class Global : INotifyPropertyChanged
    {
        public static Global Instance;

        public ObservableCollection<SongInfo> SongInfos { get; }

        public DirectoryInfo InstallFolder { get; internal set; }

        private string installPath = "";

        public string InstallPath
        {
            get => installPath;
            set
            {
                watcher.EnableRaisingEvents = false;
                InstallFolder = new DirectoryInfo(value);
                installPath = value;
                OnPropertyChanged(nameof(InstallPath));
                watcher.Path = value;
                watcher.EnableRaisingEvents = true;
            }
        }
        private double _hjsDuration = 2;

        public double HJSDuration
        {
            get => _hjsDuration;
            set
            {
                _hjsDuration = value;
                OnPropertyChanged(nameof(HJSDuration));
            }
        }

        private FileSystemWatcher watcher;

        public Logger Logger;

        public Global()
        {
            if (Instance == null)
            {
                Instance = this;
                SongInfos = new ObservableCollection<SongInfo>();
                watcher = new FileSystemWatcher
                {
                    Filter = "*.yml",
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    IncludeSubdirectories = true
                };
                watcher.Changed += FileWatcher;
                Logger = LogManager.GetLogger("global");
#if DEBUG
                InstallPath = "D:\\Games\\SteamLibrary\\steamapps\\common\\Beat Saber\\Beat Saber_Data\\CustomWIPLevels";
#endif
            }
        }

        private void FileWatcher(object sender, FileSystemEventArgs e)
        {
            lock (Instance)
            {
                Task.Delay(TimeSpan.FromMilliseconds(100)).GetAwaiter().GetResult();
                var beatmap = SongInfos.SelectMany(song => song.BeatmapSets.SelectMany(bs => bs.Beatmaps.Where(bm => bm.FullPath.Replace(".dat", ".yml") == e.FullPath))).First();
                ScriptExecutor.Execute(e.FullPath, beatmap);
                GC.Collect();
                GC.WaitForFullGCComplete();
            }
        }

        public static ImageSource GetImageSource(string fileName, string folderName)
        {
            if (Instance.InstallFolder.GetDirectories(folderName)[0].GetFiles(fileName).Length > 0)
            {
                var fileStream = Instance.InstallFolder.GetDirectories(folderName)[0].GetFiles(fileName)[0].Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return decoder.Frames[0];
            }

            return null;
        }

        public static SongInfo CheckScriptFileInitialized(SongInfo info)
        {
            var folder = Instance.InstallFolder.GetDirectories(info.SongFolderName)[0];
            foreach (var beatmapSet in info.BeatmapSets)
                foreach (var beatmap in beatmapSet.Beatmaps)
                    //if (folder.GetFiles($"{beatmap.Difficulty}{beatmapSet.Characteristic}.bw").Length > 0)
                    //{
                    //    ScriptConverter.Convert(folder.GetFiles($"{beatmap.Difficulty}{beatmapSet.Characteristic}.bw")[0]);
                    //    beatmap.ScriptNotInitialized = false;
                    //}
                    if (folder.GetFiles($"{beatmap.Difficulty}{beatmapSet.Characteristic}.yml").Length > 0)
                    {
                        beatmap.ScriptNotInitialized = false;
                    }

            GC.Collect();
            GC.WaitForFullGCComplete();
            return info;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
