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
using NLog;
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

        private const float timeConst = 0.0156f;

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

                var walls = new List<ObjectData>();
                var events = new List<EventData>();

                var firstTuple = tuples.First();

                if (firstTuple.Item1.Invariant("default"))
                {
                    var defaultGenerator = new WallGenerator();
                    var generatorList = new List<WallGenerator>();

                    Global.Instance.Logger.Info("Setting all generators");
                    setGenerators(tuples, generatorList, defaultGenerator);

                    var wallList = new List<Wall>();
                    var eventList = new List<Event>();

                    Global.Instance.Logger.Info("Generating nodes");
                    foreach (var wallGenerator in generatorList)
                    {
                        wallList.AddRange(wallGenerator.GenerateWallsFinalized());
                        eventList.AddRange(wallGenerator.GenerateEventsFinalized());
                    }

                    Global.Instance.Logger.Info("Converting nodes to objects");
                    walls.AddRange(wallList.Select(t => t.GenerateObstacle()).OrderBy(t => t.Time));
                    events.AddRange(eventList.SelectMany(t => t.GenerateEvent()).OrderBy(t => t.Time));

                    if (defaultGenerator.FlipOuterProps)
                    {
                        var eventsChanged = new int[5][] { new int[2] { 0, 0 }, new int[2] { 0, 0 }, new int[2] { 0, 0 }, new int[2] { 0, 0 }, new int[2] { 0, 0 } };
                        for (var i = 0; i <= 4; i++)
                        {
                            var highestProp = (int)defaultGenerator.GetType().GetProperties().First(f => f.Name.Invariant($"HighestProp{i}")).GetValue(defaultGenerator);
                            var lowestProp = (int)defaultGenerator.GetType().GetProperties().First(f => f.Name.Invariant($"LowestProp{i}")).GetValue(defaultGenerator);
                            var evHigh = events.Where(t => t.Type == i && t.CustomData.PropID == highestProp).ToArray();
                            var evLow = events.Where(t => t.Type == i && t.CustomData.PropID == lowestProp).ToArray();
                            foreach (var ev in events)
                            {
                                if (ev.Type == i && ev.CustomData.PropID == highestProp)
                                {
                                    ev.CustomData.PropID = lowestProp;
                                    eventsChanged[i][0]++;
                                }
                                else if (ev.Type == i && ev.CustomData.PropID == lowestProp)
                                {
                                    ev.CustomData.PropID = highestProp;
                                    eventsChanged[i][1]++;
                                }
                            }
                        }
                        Global.Instance.Logger.Info($"Changed {eventsChanged.SelectMany(t => t).Sum()} events where:{Environment.NewLine}{string.Join(Environment.NewLine, eventsChanged.Select((t, i) => $"{i} type had {t[0]} highs changed and {t[1]} lows changed."))}");
                    }
                }
                else if (firstTuple.Item1.Invariant("mergeFiles"))
                {
                    var jObjects = new List<JObject>();
                    foreach (var file in firstTuple.Item3.Find(t => t.Key.Invariant("files")).Value.Split(','))
                    {
                        jObjects.Add(JObject.Parse(File.ReadAllText(Path.Combine(beatmap.SongInfo.FullFolderPath, file))));
                    }
                    foreach (var fileJObject in jObjects)
                    {
                        var eventObjects = fileJObject["_events"].ToEvent();
                        var eventOverlap = eventObjects.Where(e => events.Any(es => e.Time.Within(es.Time, timeConst))).ToArray();
                        if (eventOverlap.Length > 0)
                            foreach (var overlappedEvent in eventOverlap)
                                Global.Instance.Logger.Log(LogLevel.Warn, $"Found event overlap with constraint of {timeConst} event data:{overlappedEvent}");
                        eventObjects.CheckZoomCountEven();
                        events.AddRange(eventObjects);
                    }
                }

                var jObject = JObject.Parse(File.ReadAllText(beatmap.FullPath));
                var oldObstaclesPath = getOldPath(beatmap, "Obstacles");
                var oldEventsPath = getOldPath(beatmap, "Events");

                if (walls.Any())
                {
                    Global.Instance.Logger.Info("Writing wall objects and storing old objects");
                    using (var s = File.OpenWrite(Path.Combine(oldObstaclesPath, $"{DateTime.Now:yy-MM-dd hh.mm.ss}.obstacles.json")))
                    using (var sw = new StreamWriter(s))
                    using (var writer = new JsonTextWriter(sw))
                    {
                        var serializer = new JsonSerializer { Formatting = Formatting.None };
                        serializer.Serialize(writer, jObject["_obstacles"]);
                    }
                    jObject["_obstacles"] = walls.ToJArray();
                }

                if (events.Any())
                {
                    Global.Instance.Logger.Info("Writing event objects and storing old objects");
                    using (var s = File.OpenWrite(Path.Combine(oldEventsPath, $"{DateTime.Now:yy-MM-dd hh.mm.ss}.events.json")))
                    using (var sw = new StreamWriter(s))
                    using (var writer = new JsonTextWriter(sw))
                    {
                        var serializer = new JsonSerializer { Formatting = Formatting.None };
                        serializer.Serialize(writer, jObject["_events"]);
                    }
                    jObject["_events"] = events.ToJArray();
                }
                beatmap.EventCount = jObject["_events"].Children().Count();
                beatmap.ObstacleCount = jObject["_obstacles"].Children().Count();
                beatmap.NoteCount = jObject["_notes"].Children().Count();

                Global.Instance.Logger.Info("Writing finalized file");
                using (var s = File.OpenWrite(beatmap.FullPath))
                using (var sw = new StreamWriter(s))
                using (var writer = new JsonTextWriter(sw))
                {
                    var serializer = new JsonSerializer { Formatting = Formatting.None };
                    serializer.Serialize(writer, jObject);
                }
            }
            catch (Exception e)
            {
                Global.Instance.Logger.Error(e, e.Message);
            }
            Global.Instance.Logger.Info($"Compleated Run {++beatmap.GenCount} of  {beatmap.SongInfo.SongName}, {beatmap.Difficulty}{beatmap.BeatmapSet.CharacteristicString}");
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

        private static void setGenerators(List<Tuple<string, string, List<KeyValuePair<string, string>>>> tuples, List<WallGenerator> generatorList, WallGenerator defaultGenerator)
        {
            var structures = new Dictionary<string, WallGenerator>();
            var rotationModes = new Dictionary<string, IRotationMode>();
            var colorModes = new Dictionary<string, ColorManager>();
            foreach (var (identifier, type, list) in tuples)
            {
                if (type == "")
                {
                    var seed = Convert.ToInt32(list.First(t => t.Key.Invariant("seed")).Value);
                    defaultGenerator.Random = new Random(seed);
                    foreach (var (key, value) in list)
                    {
                        if (!key.Invariant("seed"))
                            setFieldValue(defaultGenerator, key, value, null, null, null, null);
                    }
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
            var color = new ColorManager { Name = type };
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
            if (PointRegex.IsMatch(key) && points != null)
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
                catch (Exception e)
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
