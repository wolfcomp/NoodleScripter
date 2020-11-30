using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLog;
using NLog.Targets;
using NoodleScripter.Commands;
using NoodleScripter.Models;
using NoodleScripter.Models.BeatSaber;
using NoodleScripter.Models.NoodleScripter;
using Application = System.Windows.Forms.Application;
using Button = System.Windows.Controls.Button;
using Path = System.IO.Path;

namespace NoodleScripter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var target = LogManager.Configuration.FindTargetByName<MemoryTarget>("logmem");
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NoodleScripter.Models.NoodleScripter.template.yml"))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    ScriptExecutor.Template = streamReader.ReadToEnd();
                }
            }
            Task.Run(async () =>
            {
                while (true)
                {
                    var logs = target.Logs.Count > 4 ? target.Logs.Skip(target.Logs.Count - 4) : target.Logs;
                    Global.Instance.LogBoxString = string.Join(Environment.NewLine, logs);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            });
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            Global.Instance.InstallPath = dialog.SelectedPath;
            Button_Refresh(sender, e);
        }

        private void Button_Refresh(object sender, RoutedEventArgs e)
        {
            Global.Instance.Logger.Info("Getting all beatmaps");
            Global.Instance.SongInfos.Clear();
            Task.Run(() =>
            {
                foreach (var directoryInfo in Global.Instance.InstallFolder.GetDirectories())
                {
                    foreach (var fileInfo in directoryInfo.GetFiles("*.dat"))
                    {
                        if (fileInfo.Name.ToLowerInvariant().Contains("info"))
                        {
                            var map = Global.CheckScriptFileInitialized(SongInfo.GetInfo(fileInfo));
                            Dispatcher.Invoke(() =>
                            {
                                Global.Instance.SongInfos.Add(map);
                            });
                        }
                    }
                }
                Global.Instance.Logger.Info("Got all beatmaps");
            });
        }

        private void Button_Initialize(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var beatmap = button.DataContext as Beatmap;
            Global.Instance.Logger.Info($"Initializing {beatmap.SongInfo.SongName}, {beatmap.Difficulty}{beatmap.BeatmapSet.CharacteristicString}");
            ScriptExecutor.Initialize(beatmap);
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        private void Button_RunScript(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var beatmap = button.DataContext as Beatmap;
            Global.Instance.Logger.Info($"Generating {beatmap.SongInfo.SongName}, {beatmap.Difficulty}{beatmap.BeatmapSet.CharacteristicString}");
            Task.Run(() =>
            {
                ScriptExecutor.Execute(beatmap);
                GC.Collect();
                GC.WaitForFullGCComplete();
            });
        }
    }
}
