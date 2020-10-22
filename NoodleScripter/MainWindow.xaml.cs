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
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NoodleScripter.Models.NoodleScripter.template.yml"))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    ScriptExecutor.Template = streamReader.ReadToEnd();
                }
            }
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
            Global.Instance.SongInfos.Clear();
            foreach (var directoryInfo in Global.Instance.InstallFolder.GetDirectories())
            {
                foreach (var fileInfo in directoryInfo.GetFiles("*.dat"))
                {
                    if (fileInfo.Name.ToLowerInvariant().Contains("info"))
                    {
                        Global.Instance.SongInfos.Add(Global.CheckScriptFileInitialized(SongInfo.GetInfo(fileInfo)));
                    }
                }
            }
        }

        private void Button_Initialize(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var beatmap = button.DataContext as Beatmap;
            ScriptExecutor.Initialize(beatmap);
            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        private void Button_RunScript(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var beatmap = button.DataContext as Beatmap;
            ScriptExecutor.Execute(beatmap);
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }
}
