using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoodleScripter.Models.BeatSaber;
using NoodleScripter.Models.NoodleScripter;
using Color = NoodleScripter.Models.NoodleScripter.Color;

// ReSharper disable PatternAlwaysOfType

namespace NoodleScripter.Commands
{
    public class ScriptExecutor
    {
        public static string Template;

        public static void Initialize(Beatmap beatmap)
        {
            var path = Path.Combine(Global.Instance.InstallPath, beatmap.SongInfo.SongFolderName, $"{beatmap.Difficulty}{beatmap.BeatmapSet.CharacteristicString}.yml");
            File.WriteAllText(path,
            Template.Replace("${beatmapname}", beatmap.SongInfo.SongName)
                .Replace("${beatmapdifficulty}", beatmap.Difficulty.ToString())
                .Replace("${beatmapcharacteristic}", beatmap.BeatmapSet.CharacteristicString)
                .Replace("${randomseed}", new Random().Next().ToString()));
            beatmap.ScriptNotInitialized = false;
        }

        public static void Execute(string fullPath, Beatmap beatmap)
        {
            try
            {
                var file = File.Open(fullPath.Replace(".dat", ".yml"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var streamFile = new StreamReader(file);
                var info = streamFile.ReadAllLines().Where(t => !t.Trim().StartsWith("#")).Select(t => t.Trim().ToLowerInvariant().Split(':'));
                streamFile.Dispose();
                file.Dispose();
                var tuples = new List<Tuple<string, string, List<KeyValuePair<string, string>>>>();
                var isStart = true;
                var curIndex = 0;
                var defaultGenerator = new WallGenerator();
                var generatorList = new List<WallGenerator>();
                var structures = new Dictionary<string, WallGenerator>();
                var wallList = new List<Wall>();
                var rotationModes = new Dictionary<string, IRotationMode>();
                var colorModes = new Dictionary<string, Color>();
                foreach (var infoStrings in info)
                {
                    if (isStart && infoStrings.Length != 1)
                    {
                        isStart = false;
                        tuples.Add(new Tuple<string, string, List<KeyValuePair<string, string>>>(infoStrings[0].Trim(), infoStrings[1].Trim(), new List<KeyValuePair<string, string>>()));
                        curIndex = tuples.Count - 1;
                    }
                    else
                    {
                        if (infoStrings.Length == 1)
                        {
                            isStart = true;
                            continue;
                        }

                        tuples[curIndex].Item3.Add(new KeyValuePair<string, string>(infoStrings[0].Trim(), infoStrings[1].Trim()));
                    }
                }

                foreach (var (identifier, type, list) in tuples)
                {
                    if (type == "")
                    {
                        var seed = Convert.ToInt32(list.First(t => t.Key.Invariant("seed")).Value);
                        defaultGenerator.Random = new Random(seed);
                    }
                    else if (identifier.Invariant("rotationMode"))
                    {
                        var local = false;
                        var multi = false;
                        var localRotList = new List<Tuple<Vector3D, double>>();
                        var rotList = new List<Tuple<Vector3D, double>>();
                        dynamic rotationStore = new NoRotation();
                        foreach (var (key, value) in list)
                        {
                            if (key.Invariant("localrotation"))
                            {
                                local = true;

                                if (value == "")
                                {
                                    multi = true;
                                    continue;
                                }
                            }
                            else if (key.Invariant("rotation"))
                            {
                                local = false;

                                if (value == "")
                                {
                                    multi = true;
                                    continue;
                                }
                            }

                            if (key.Invariant("type")) rotationStore = value.ToLowerInvariant() switch
                            {
                                "static" => new StaticRotation(),
                                "ease" => new EaseRotation(),
                                "random" => new RandomRotation(),
                                "switch" => new SwitchRotation(),
                                _ => throw new ArgumentOutOfRangeException("Rotation type can only be of types: Static, Ease, Random, Switch")
                            };
                            else if (key.Invariant("ease"))
                            {
                                rotationStore.Easing = value.GetEasing();
                            }
                            else
                            {
                                if (multi)
                                {
                                    switch (rotationStore)
                                    {
                                        case EaseRotation _:
                                            var vector = value.Substring(0, value.LastIndexOf(','));
                                            var time = Convert.ToDouble(value.Substring(value.LastIndexOf(',') + 1));
                                            if (local)
                                                localRotList.Add(new Tuple<Vector3D, double>(Vector3D.Parse(vector), time));
                                            else
                                                rotList.Add(new Tuple<Vector3D, double>(Vector3D.Parse(vector), time));
                                            break;
                                        case RandomRotation _:
                                            if (local)
                                                localRotList.Add(new Tuple<Vector3D, double>(Vector3D.Parse(value), 0));
                                            else
                                                rotList.Add(new Tuple<Vector3D, double>(Vector3D.Parse(value), 0));
                                            break;
                                        case SwitchRotation _:
                                            if (local)
                                                localRotList.Add(new Tuple<Vector3D, double>(Vector3D.Parse(value), 0));
                                            else
                                                rotList.Add(new Tuple<Vector3D, double>(Vector3D.Parse(value), 0));
                                            break;
                                    }
                                }
                                else
                                {
                                    if (local)
                                        rotationStore.LocalRotation = Vector3D.Parse(value);
                                    else
                                        rotationStore.Rotation = Vector3D.Parse(value);
                                }
                            }
                        }
                        switch (rotationStore)
                        {
                            case EaseRotation easeStore:
                                easeStore.LocalRotations = localRotList.ToArray();
                                easeStore.Rotations = rotList.ToArray();
                                break;
                            case RandomRotation randomStore:
                                randomStore.LocalRotation = new Tuple<Vector3D, Vector3D>(localRotList[0].Item1, localRotList[1].Item1);
                                randomStore.Rotation = new Tuple<Vector3D, Vector3D>(rotList[0].Item1, rotList[1].Item1);
                                break;
                            case SwitchRotation switchStore:
                                switchStore.LocalRotations = localRotList.Select(t => t.Item1).ToArray();
                                switchStore.Rotations = rotList.Select(t => t.Item1).ToArray();
                                break;
                        }
                        rotationModes.Add(type, rotationStore);
                    }
                    else if (identifier.Invariant("colorMode"))
                    {
                        var local = false;
                        var color = new Color();
                        var colorList = new List<Tuple<System.Windows.Media.Color, double>>();
                        foreach (var (key, value) in list)
                        {
                            if (key.StartsWith("p"))
                            {
                                var values = value.Split(',');
                                var colorType = values[0];
                                var time = Convert.ToDouble(values[1]);
                                colorList.Add(new Tuple<System.Windows.Media.Color, double>((System.Windows.Media.Color)ColorConverter.ConvertFromString(colorType), time));
                            }
                            else if (key.Invariant("type"))
                            {
                                if (value.Invariant("Single"))
                                    color.Type = ColorType.Single;
                                else if (value.Invariant("Gradient"))
                                    color.Type = ColorType.Gradient;
                                else if (value.Invariant("Flash"))
                                    color.Type = ColorType.Flash;
                                else if (value.Invariant("Rainbow"))
                                    color.Type = ColorType.Rainbow;
                                else
                                    throw new ArgumentOutOfRangeException("Color mode type can only be of types: Single, Gradient, Flash, Rainbow");
                            }
                            else if (key.Invariant("repetitions"))
                            {
                                color.Repetitions = Convert.ToSingle(value);
                            }
                        }

                        color.Colors = colorList.ToArray();
                        colorModes.Add(type, color);
                    }
                    else if (float.TryParse(identifier, out var beat))
                    {
                        var generator = getGenerator(type.Replace("wall", "WallGenerator"));
                        var points = new List<Vector3D>();
                        generator = generator.GetWallGenerator(defaultGenerator);
                        generator.Beat = beat;
                        foreach (var (key, value) in list)
                        {
                            setFieldValue(generator, key, value, points, rotationModes, colorModes, structures);
                        }
                        generatorList.Add(generator);
                    }
                    else
                    {
                        var generator = getGenerator(type.Replace("wall", "WallGenerator"));
                        var points = new List<Vector3D>();
                        generator = generator.GetWallGenerator(defaultGenerator);
                        foreach (var (key, value) in list)
                        {
                            setFieldValue(generator, key, value, points, rotationModes, colorModes, structures);
                        }
                        structures.Add(identifier, generator);
                    }
                }

                foreach (var wallGenerator in generatorList)
                {
                    wallList.AddRange(wallGenerator.GenerateWallsFinalized());
                }

                var walls = wallList.Select(t => t.GenerateObstacle()).ToArray();
                var jObject = JObject.Parse(File.ReadAllText(beatmap.FullPath));
                var oldPath = Path.Combine(beatmap.FullPath, "oldObstacles").Replace(".dat", "");
                if (!Directory.Exists(oldPath)) Directory.CreateDirectory(oldPath);
                var files = Directory.GetFiles(oldPath);
                if (files.Length > 19)
                {
                    foreach (var s in files.Take(files.Length - 19))
                    {
                        File.Delete(s);
                    }
                }

                File.WriteAllText(Path.Combine(oldPath, $"{DateTime.Now:yy-MM-dd hh.mm.ss}.json"), jObject["_obstacles"].ToString(Formatting.None));
                jObject["_obstacles"] = walls.ToJArray();
                File.WriteAllText(beatmap.FullPath, jObject.ToString(Formatting.None));
            }
            catch (Exception e)
            {
                Global.Instance.Logger.Error(e, e.Message);
            }
        }

        private static void setFieldValue(WallGenerator generator, string key, string value, List<Vector3D> points, Dictionary<string, IRotationMode> rotationModes, Dictionary<string, Color> colorModes, Dictionary<string, WallGenerator> structures)
        {
            if (key.StartsWith("p"))
            {
                points.Add(Vector3D.Parse(value));
                generator.GetType().GetProperty("Points").SetValue(generator, points.ToArray());
            }
            else
            {
                var field = generator.GetType().GetProperties().First(f => key.Invariant(f.Name));
                switch (field.PropertyType)
                {
                    case Type vector3DType when vector3DType == typeof(Vector3D):
                        field.SetValue(generator, Vector3D.Parse(value));
                        break;
                    case Type rotationModeType when rotationModeType == typeof(IRotationMode):
                        field.SetValue(generator, rotationModes[value]);
                        break;
                    case Type nullableIntType when nullableIntType == typeof(int?):
                        field.SetValue(generator, Convert.ToInt32(value));
                        break;
                    case Type nullableDoubleType when nullableDoubleType == typeof(double?):
                        field.SetValue(generator, Convert.ToDouble(value));
                        break;
                    case Type mirrorPointType when mirrorPointType == typeof(MirrorPoint):
                        field.SetValue(generator, (MirrorPoint)Convert.ToInt32(value));
                        break;
                    case Type mirrorTypeType when mirrorTypeType == typeof(MirrorType):
                        field.SetValue(generator, (MirrorType)Convert.ToInt32(value));
                        break;
                    case Type mirrorTypeType when mirrorTypeType == typeof(CurveType):
                        field.SetValue(generator, (CurveType)Convert.ToInt32(value));
                        break;
                    case Type mirrorTypeType when mirrorTypeType == typeof(Random):
                        field.SetValue(generator, new Random(Convert.ToInt32(value)));
                        break;
                    case Type colorModesType when colorModesType == typeof(Color):
                        field.SetValue(generator, colorModes[value]);
                        break;
                    case Type easingsType when easingsType == typeof(Easings):
                        field.SetValue(generator, value.GetEasing());
                        break;
                    case Type structuresType when structuresType == typeof(List<WallGenerator>):
                        foreach (var s in value.Split(',').Where(t => !string.IsNullOrWhiteSpace(t)))
                        {
                            generator.Structures.Add(structures[s]);
                        }
                        break;
                    default:
                        var fieldValue = Convert.ChangeType(value, field.PropertyType);
                        field.SetValue(generator, fieldValue);
                        break;
                }
            }
        }

        private static WallGenerator getGenerator(string type)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "NoodleScripter.Models.NoodleScripter").ToArray();
            return (WallGenerator)Activator.CreateInstance(types.First(t => string.Equals(t.Name, type, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}
