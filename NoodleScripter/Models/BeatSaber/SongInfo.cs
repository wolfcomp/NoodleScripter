using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json.Linq;

namespace NoodleScripter.Models.BeatSaber
{
    internal class SongInfoExt
    {
        public static BeatmapDifficulty GetDifficulty(string dif) =>
            dif switch
            {
                "ExpertPlus" => BeatmapDifficulty.ExpertPlus,
                "Expert" => BeatmapDifficulty.Expert,
                "Hard" => BeatmapDifficulty.Hard,
                "Normal" => BeatmapDifficulty.Normal,
                "Easy" => BeatmapDifficulty.Easy,
                _ => throw new ArgumentException("Invalid difficulty in beatmap")
            };

        public static BeatmapCharacteristic GetCharacteristic(string dif) =>
            dif switch
            {
                "Standard" => BeatmapCharacteristic.Standard,
                "Lawless" => BeatmapCharacteristic.Lawless,
                "Lightshow" => BeatmapCharacteristic.Lightshow,
                "90Degree" => BeatmapCharacteristic.Degree90,
                "360Degree" => BeatmapCharacteristic.Degree360,
                "NoArrows" => BeatmapCharacteristic.NoArrows,
                "OneSaber" => BeatmapCharacteristic.OneSaber,
                _ => throw new ArgumentException("Invalid difficulty in beatmap")
            };
    }

    public struct SongInfo
    {
        public string SongName { get; }
        public string Mapper { get; }
        public string CoverFileName { get; }
        public string SongFolderName { get; }

        public ImageSource Cover => Global.GetImageSource(CoverFileName, SongFolderName);

        public BeatmapSet[] BeatmapSets { get; }

        public SongInfo(string songName, string mapper, string coverFileName, string songFolderName, BeatmapSet[] beatmapSets) : this()
        {
            SongName = songName;
            Mapper = mapper;
            CoverFileName = coverFileName;
            BeatmapSets = beatmapSets;
            SongFolderName = songFolderName;
            foreach (var beatmapSet in beatmapSets)
            {
                beatmapSet.SongInfo = this;
            }
        }

        public static SongInfo GetInfo(FileInfo file)
        {
            var info = JObject.Parse(file.OpenText().ReadToEnd());
            var beatmaps = new List<Beatmap>();
            var beatmapsets = new List<BeatmapSet>();
            foreach (var beatmapsetJt in info["_difficultyBeatmapSets"])
            {
                if (beatmapsetJt is JObject beatmapset)
                {
                    foreach (var beatmapJt in beatmapset["_difficultyBeatmaps"])
                    {
                        if (beatmapJt is JObject beatmap)
                        {
                            beatmaps.Add(new Beatmap(beatmap["_beatmapFilename"].ToString(), beatmap["_noteJumpMovementSpeed"].ToObject<int>(), beatmap["_noteJumpStartBeatOffset"].ToObject<double>(), SongInfoExt.GetDifficulty(beatmap["_difficulty"].ToString())));
                        }
                    }
                    beatmapsets.Add(new BeatmapSet(beatmaps.ToArray(), SongInfoExt.GetCharacteristic(beatmapset["_beatmapCharacteristicName"].ToString())));
                }
                beatmaps.Clear();
            }
            return new SongInfo(info["_songName"].ToString(), info["_levelAuthorName"].ToString(), info["_coverImageFilename"].ToString(), file.Directory.Name, beatmapsets.ToArray());
        }
    }

    public class BeatmapSet
    {
        public BeatmapCharacteristic Characteristic { get; }
        public string CharacteristicString => Characteristic.GetEnumString();
        public Beatmap[] Beatmaps { get; }
        public SongInfo SongInfo { get; set; }

        public BeatmapSet(Beatmap[] beatmaps, BeatmapCharacteristic characteristic)
        {
            Characteristic = characteristic;
            Beatmaps = beatmaps;
            foreach (var beatmap in Beatmaps)
            {
                beatmap.BeatmapSet = this;
            }
        }
    }

    public class Beatmap : INotifyPropertyChanged
    {
        private bool _scriptNotInitialized = true;
        public BeatmapDifficulty Difficulty { get; }
        public int NJS { get; }
        public double Offset { get; }
        public string FileName { get; }
        public string FullPath => Path.Combine(Global.Instance.InstallPath, SongInfo.SongFolderName, FileName);

        public bool ScriptNotInitialized
        {
            get => _scriptNotInitialized;
            set
            {
                _scriptNotInitialized = value;
                OnPropertyChanged(nameof(ScriptNotInitialized));
            }
        }

        public BeatmapSet BeatmapSet { get; set; }
        public SongInfo SongInfo => BeatmapSet.SongInfo;

        public Beatmap(string fileName, int njs, double offset, BeatmapDifficulty difficulty)
        {
            FileName = fileName;
            NJS = njs;
            Offset = offset;
            Difficulty = difficulty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum BeatmapCharacteristic
    {
        Standard,
        Lawless,
        Lightshow,
        Degree90,
        Degree360,
        NoArrows,
        OneSaber
    }

    public enum BeatmapDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert,
        ExpertPlus
    }
}
