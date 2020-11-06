using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoodleScripter.Models.BeatSaber;
using NoodleScripter.Models.NoodleScripter;
using Color = NoodleScripter.Models.NoodleScripter.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

// ReSharper disable PatternAlwaysOfType

namespace NoodleScripter.Commands
{
    public class ScriptExecutor
    {
        public static string Template;

        public static Regex PointRegex = new Regex("^p([0-9])", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        private static Calculator calculator = new Calculator();

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

        public static void Execute(Beatmap beatmap)
        {
            try
            {
                var tuples = new List<Tuple<string, string, List<KeyValuePair<string, string>>>>();

                groupYmlContextLines(readAllLinesFromFile(beatmap.FullPath.Replace(".dat", ".yml")), tuples, beatmap.SongInfo.FullFolderPath);

                var generatorList = new List<WallGenerator>();

                setGenerators(tuples, generatorList);

                var wallList = new List<Wall>();
                var eventList = new List<Event>();

                foreach (var wallGenerator in generatorList)
                {
                    wallList.AddRange(wallGenerator.GenerateWallsFinalized());
                    eventList.AddRange(wallGenerator.GenerateEventsFinalized());
                }

                var walls = wallList.Select(t => t.GenerateObstacle()).OrderBy(t => t.Time).ToArray();
                var events = eventList.Select(t => t.GenerateEvent()).OrderBy(t => t.Time).ToArray();
                var jObject = JObject.Parse(File.ReadAllText(beatmap.FullPath));
                var oldObstaclesPath = getOldPath(beatmap, "Obstacles");
                var oldEventsPath = getOldPath(beatmap, "Events");

                if (walls.Any())
                {
                    File.WriteAllText(Path.Combine(oldObstaclesPath, $"{DateTime.Now:yy-MM-dd hh.mm.ss}.obstacles.json"), jObject["_obstacles"].ToString(Formatting.None));
                    jObject["_obstacles"] = walls.ToJArray();
                }

                if (events.Any())
                {
                    File.WriteAllText(Path.Combine(oldEventsPath, $"{DateTime.Now:yy-MM-dd hh.mm.ss}.events.json"), jObject["_events"].ToString(Formatting.None));
                    jObject["_events"] = events.ToJArray();
                }
                beatmap.EventCount = jObject["_events"].Children().Count();
                beatmap.ObstacleCount = jObject["_obstacles"].Children().Count();
                beatmap.NoteCount = jObject["_notes"].Children().Count();

                File.WriteAllText(beatmap.FullPath, jObject.ToString(Formatting.None));
            }
            catch (Exception e)
            {
                Global.Instance.Logger.Error(e, e.Message);
            }
        }

        private static string getOldPath(Beatmap beatmap, string type)
        {
            var path = Path.Combine(beatmap.FullPath, $"old{type}").Replace(".dat", "");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            cleanOverStorage(path);
            return path;
        }

        private static void cleanOverStorage(string path)
        {
            var files = Directory.GetFiles(path);
            if (files.Length > 19)
            {
                foreach (var s in files.Take(files.Length - 19))
                {
                    File.Delete(s);
                }
            }
        }

        private static void setGenerators(List<Tuple<string, string, List<KeyValuePair<string, string>>>> tuples, List<WallGenerator> generatorList)
        {
            var defaultGenerator = new WallGenerator();
            var structures = new Dictionary<string, WallGenerator>();
            var rotationModes = new Dictionary<string, IRotationMode>();
            var colorModes = new Dictionary<string, ColorManager>();
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
                    try
                    {
                        rotationModes.Add(type, rotationStore);
                    }
                    catch (Exception e)
                    {
                        Global.Instance.Logger.Error($"Could not add rotationMode name: {type}, as it already exists");
                        throw e;
                    }
                }
                else if (identifier.Invariant("colorMode"))
                {
                    setColorMode(colorModes, type, list);
                }
                else
                {
                    if (!tryGetGenerator(type.Replace("wall", "WallGenerator").Replace("event", "EventGenerator"), out var generator))
                        try
                        {
                            generator = structures[type].Copy();
                        }
                        catch (Exception e)
                        {
                            Global.Instance.Logger.Error($"Could not find the key: {type}");
                            throw e;
                        }

                    var points = new List<Vector3D>();
                    generator = generator.GetWallGenerator(defaultGenerator);
                    foreach (var (key, value) in list)
                    {
                        setFieldValue(generator, key, value, points, rotationModes, colorModes, structures);
                    }

                    if (float.TryParse(identifier, out var beat))
                    {
                        generator.Beat = beat;
                        generatorList.Add(generator);
                    }
                    else
                    {
                        try
                        {
                            structures.Add(identifier, generator);
                        }
                        catch (Exception e)
                        {
                            Global.Instance.Logger.Error($"Could not add structure name: {identifier}, as it already exists");
                            throw e;
                        }
                    }
                }
            }
        }

        private static void setColorMode(Dictionary<string, ColorManager> colorModes, string type, List<KeyValuePair<string, string>> list)
        {
            var color = new ColorManager();
            var colorList = new List<Tuple<Color, double>>();
            foreach (var (key, value) in list)
            {
                if (key.StartsWith("p"))
                {
                    var values = value.Split(',');
                    if (values.Length == 2)
                    {
                        colorList.Add(new Tuple<Color, double>(
                            // ReSharper disable once PossibleNullReferenceException
                            ((System.Windows.Media.Color)ColorConverter.ConvertFromString(values[0])).GetScriptColor(),
                            Convert.ToDouble(values[1])));
                    }
                    else if (values.Length == 1)
                    {
                        colorList.Add(new Tuple<Color, double>(
                            // ReSharper disable once PossibleNullReferenceException
                            ((System.Windows.Media.Color)ColorConverter.ConvertFromString(values[0])).GetScriptColor(),
                            0));
                    }
                    else if (values.Length == 4)
                    {
                        colorList.Add(new Tuple<Color, double>(
                            // ReSharper disable once PossibleNullReferenceException
                            Color.FromRgb(Convert.ToDouble(values[0]), Convert.ToDouble(values[1]), Convert.ToDouble(values[2])),
                            Convert.ToDouble(values[3])));
                    }
                    else if (values.Length == 5)
                    {
                        colorList.Add(new Tuple<Color, double>(
                            // ReSharper disable once PossibleNullReferenceException
                            Color.FromArgb(Convert.ToDouble(values[0]), Convert.ToDouble(values[1]), Convert.ToDouble(values[2]), Convert.ToDouble(values[3])),
                            Convert.ToDouble(values[4])));
                    }
                    else
                        throw new ArgumentOutOfRangeException("p", "Color can only be of Name, Hex, 3 double, 4 double and end with time if not Name or Hex");
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
                        throw new ArgumentOutOfRangeException("type", "Color mode type can only be of types: Single, Gradient, Flash, Rainbow");
                }
                else if (key.Invariant("repetitions"))
                {
                    color.Repetitions = Convert.ToSingle(value);
                }
                else if (key.Invariant("easing"))
                {
                    color.Easing = value.GetEasing();
                }
            }

            color.Colors = colorList.ToArray();
            try
            {
                colorModes.Add(type, color);
            }
            catch (Exception e)
            {
                Global.Instance.Logger.Error($"Could not add colorMode name: {type}, as it already exists");
                throw e;
            }
        }

        private static void groupYmlContextLines(string[][] input, List<Tuple<string, string, List<KeyValuePair<string, string>>>> tuples, string path)
        {
            var isStart = true;
            var curIndex = 0;
            foreach (var infoStrings in input)
            {
                if (infoStrings[0].Trim().Invariant("importFile")) groupYmlContextLines(readAllLinesFromFile(Path.Combine(path, infoStrings[1].Trim())), tuples, path);
                else
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
            }
        }

        private static string[][] readAllLinesFromFile(string fileName)
        {
            var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var streamFile = new StreamReader(file);
            var info = streamFile.ReadAllLines().Where(t => !t.Trim().StartsWith("#")).Select(t => t.Trim().ToLowerInvariant().Split(':')).ToArray();
            streamFile.Dispose();
            file.Dispose();
            return info;
        }

        private static void setFieldValue(WallGenerator generator, string key, string value, List<Vector3D> points, Dictionary<string, IRotationMode> rotationModes, Dictionary<string, ColorManager> colorModes, Dictionary<string, WallGenerator> structures)
        {
            if (PointRegex.IsMatch(key))
            {
                points.Add(Vector3D.Parse(value));
                generator.GetType().GetProperty("Points").SetValue(generator, points.ToArray());
            }
            else
            {
                try
                {
                    var field = generator.GetType().GetProperties().First(f => key.Invariant(f.Name));
                    var propertyType = Nullable.GetUnderlyingType(field.PropertyType) ?? field.PropertyType;
                    switch (propertyType)
                    {
                        case Type vector3DType when vector3DType == typeof(Vector3D):
                            field.SetValue(generator, Vector3D.Parse(value));
                            break;
                        case Type rotationModeType when rotationModeType == typeof(IRotationMode):
                            try
                            {
                                field.SetValue(generator, rotationModes[value]);
                            }
                            catch (Exception e)
                            {
                                Global.Instance.Logger.Error($"Could not find the key: {value}");
                                throw e;
                            }
                            break;
                        case Type mirrorPointType when mirrorPointType == typeof(MirrorPoint):
                            field.SetValue(generator, (MirrorPoint)Convert.ToInt32(value));
                            break;
                        case Type mirrorType when mirrorType == typeof(MirrorType):
                            field.SetValue(generator, (MirrorType)Convert.ToInt32(value));
                            break;
                        case Type curveType when curveType == typeof(CurveType):
                            field.SetValue(generator, (CurveType)Convert.ToInt32(value));
                            break;
                        case Type eventType when eventType == typeof(EventType):
                            field.SetValue(generator, (EventType)Convert.ToInt32(value));
                            break;
                        case Type lightType when lightType == typeof(LightType):
                            field.SetValue(generator, (LightType)Convert.ToInt32(value));
                            break;
                        case Type randomType when randomType == typeof(Random):
                            field.SetValue(generator, new Random(Convert.ToInt32(value)));
                            break;
                        case Type colorModesType when colorModesType == typeof(ColorManager):
                            try
                            {
                                field.SetValue(generator, colorModes[value]);
                            }
                            catch (Exception e)
                            {
                                Global.Instance.Logger.Error($"Could not find the key: {value}");
                                throw e;
                            }
                            break;
                        case Type easingsType when easingsType == typeof(Easings):
                            field.SetValue(generator, value.GetEasing());
                            break;
                        case Type structuresType when structuresType == typeof(List<WallGenerator>):
                            foreach (var s in value.Split(',').Where(t => !string.IsNullOrWhiteSpace(t)))
                            {
                                try
                                {
                                    generator.Structures.Add(structures[s].Copy());
                                }
                                catch (Exception e)
                                {
                                    Global.Instance.Logger.Error($"Could not find the key: {s}");
                                    throw e;
                                }
                            }
                            break;
                        case Type integerType when integerType == typeof(double) ||
                                                   integerType == typeof(float) ||
                                                   integerType == typeof(int) ||
                                                   integerType == typeof(uint) ||
                                                   integerType == typeof(long) ||
                                                   integerType == typeof(ulong) ||
                                                   integerType == typeof(short) ||
                                                   integerType == typeof(ushort):
                            var calculatedValue = Convert.ChangeType(calculator.Solve(value), propertyType);
                            field.SetValue(generator, calculatedValue);
                            break;
                        default:
                            var fieldValue = Convert.ChangeType(value, propertyType);
                            field.SetValue(generator, fieldValue);
                            break;
                    }
                }
                catch(Exception e)
                {
                    Global.Instance.Logger.Error($"Could not process key: {key}");
                    throw e;
                }
            }
        }

        private static bool tryGetGenerator(string type, out WallGenerator generator)
        {
            generator = null;
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "NoodleScripter.Models.NoodleScripter").ToArray();
            var generatorType = types.FirstOrDefault(t => string.Equals(t.Name, type, StringComparison.InvariantCultureIgnoreCase));
            if (generatorType == null) return false;
            generator = (WallGenerator)Activator.CreateInstance(generatorType);
            return true;
        }

        public static void GetAllYmlFilesForBeatmap(Beatmap beatmap, FileInfo file)
        {
            void getYmlFiles(string path)
            {
                foreach (var strings in readAllLinesFromFile(path))
                {
                    if (strings[0].Trim().Invariant("importFile"))
                    {
                        beatmap.YmlFiles.Add(strings[1].Trim());
                        getYmlFiles(Path.Combine(file.DirectoryName, strings[1].Trim()));
                    }
                }
            }
            beatmap.YmlFiles.Add(file.Name);
            getYmlFiles(file.FullName);
        }
    }
}
