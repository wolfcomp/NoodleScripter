using NoodleScripter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoodleScripter.Models.BeatSaber;
using NoodleScripter.Models.NoodleScripter;
using Color = System.Windows.Media.Color;
using Vector3DConverter = NoodleScripter.Models.BeatSaber.Vector3DConverter;

namespace NoodleScripter
{
    public static class Extension
    {
        public static string GetEnumString(this BeatmapCharacteristic characteristic) => characteristic switch
        {
            BeatmapCharacteristic.Standard => "Standard",
            BeatmapCharacteristic.Lawless => "Lawless",
            BeatmapCharacteristic.Lightshow => "Lightshow",
            BeatmapCharacteristic.Degree90 => "90Degree",
            BeatmapCharacteristic.Degree360 => "360Degree",
            BeatmapCharacteristic.NoArrows => "NoArrows",
            BeatmapCharacteristic.OneSaber => "OneSaber",
            _ => throw new ArgumentOutOfRangeException(nameof(characteristic), characteristic, null)
        };

        public static string GetJsonString(this ISerializable serializable)
        {
            return JsonConvert.SerializeObject(serializable, Settings);
        }

        public static string GetJsonString(this ISerializable[] serializable)
        {
            return JsonConvert.SerializeObject(serializable, Settings);
        }

        public static JArray ToJArray(this ISerializable[] serializable)
        {
            return JArray.FromObject(serializable, JsonSerializer.Create(Settings));
        }

        private static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Vector3DConverter(),
                new VectorConverter(),
                new NullableVector3DConverter()
            }
        };

        public static Easings GetEasing(this string str) => str.ToLowerInvariant() switch
        {
            "linear" => Easings.Linear,
            "easeinquad" => Easings.EaseInQuad,
            "easeoutquad" => Easings.EaseOutQuad,
            "easeinoutquad" => Easings.EaseInOutQuad,
            "easeincubic" => Easings.EaseInCubic,
            "easeoutcubic" => Easings.EaseOutCubic,
            "easeinoutcubic" => Easings.EaseInOutCubic,
            "easeinquart" => Easings.EaseInQuart,
            "easeoutquart" => Easings.EaseOutQuart,
            "easeinoutquart" => Easings.EaseInOutQuart,
            "easeinquint" => Easings.EaseInQuint,
            "easeoutquint" => Easings.EaseOutQuint,
            "easeinoutquint" => Easings.EaseInOutQuint,
            "easeinsine" => Easings.EaseInSine,
            "easeoutsine" => Easings.EaseOutSine,
            "easeinoutsine" => Easings.EaseInOutSine,
            "easeincirc" => Easings.EaseInCirc,
            "easeoutcirc" => Easings.EaseOutCirc,
            "easeinoutcirc" => Easings.EaseInOutCirc,
            "easeinback" => Easings.EaseInBack,
            "easeoutback" => Easings.EaseOutBack,
            "easeinoutback" => Easings.EaseInOutBack,
            "easeinelastic" => Easings.EaseInElastic,
            "easeoutelastic" => Easings.EaseOutElastic,
            "easeinoutelastic" => Easings.EaseInOutElastic,
            "easeinbounce" => Easings.EaseInBounce,
            "easeoutbounce" => Easings.EaseOutBounce,
            "easeinoutbounce" => Easings.EaseInOutBounce,
            _ => throw new ArgumentOutOfRangeException()
        };

        public static Vector3D GetVector(this Color color)
        {
            var r = (color.R * (color.A / 255D)) / 255D;
            var g = (color.G * (color.A / 255D)) / 255D;
            var b = (color.B * (color.A / 255D)) / 255D;
            return new Vector3D(r, g, b);
        }

        public static IEnumerable<Wall> MirrorGenerator<T>(this IEnumerable<Wall> walls, WallGenerator<T> generator)
        {
            var ret = walls.ToList();
            if (generator.Mirror.HasFlag(MirrorPoint.X))
            {
                if (generator.MirrorType.HasFlag(MirrorType.Add))
                {
                    ret.AddRange(ret.ToArray().Select(t =>
                    {
                        var m = t.Copy();
                        m.StartRow = -t.StartRow;
                        m.Width = -t.Width;
                        m.LocalRotation = new Vector3D(t.LocalRotation.X, -t.LocalRotation.Y, -t.LocalRotation.Z);
                        return m;
                    }));
                }
                else
                {
                    ret = ret.Select(t =>
                    {
                        var m = t.Copy();
                        m.StartRow = -t.StartRow;
                        m.Width = -t.Width;
                        m.LocalRotation = new Vector3D(t.LocalRotation.X, -t.LocalRotation.Y, -t.LocalRotation.Z);
                        return m;
                    }).ToList();
                }
            }
            if (generator.Mirror.HasFlag(MirrorPoint.Y))
            {
                if (generator.MirrorType.HasFlag(MirrorType.Add))
                {
                    ret.AddRange(ret.ToArray().Select(t =>
                    {
                        var m = t.Copy();
                        m.StartHeight = -t.StartHeight;
                        m.Height = -t.Height;
                        m.Rotation = new Vector3D(-t.Rotation.X, t.Rotation.Y, t.Rotation.Z);
                        m.LocalRotation = new Vector3D(t.LocalRotation.X, -t.LocalRotation.Y, -t.LocalRotation.Z);
                        if (Math.Abs(m.LocalRotation.Z) > 0)
                        {
                            m.StartRow += m.StartRow > 0 ? m.Height : -m.Height;
                            m.StartRow += m.Width;
                        }
                        return m;
                    }));
                }
                else
                {
                    ret = ret.Select(t =>
                    {
                        var m = t.Copy();
                        m.StartHeight = -t.StartHeight;
                        m.Height = -t.Height;
                        m.LocalRotation = new Vector3D(t.LocalRotation.X, -t.LocalRotation.Y, -t.LocalRotation.Z);
                        return m;
                    }).ToList();
                }
            }
            if (generator.Mirror.HasFlag(MirrorPoint.Z))
            {
                if (generator.MirrorType.HasFlag(MirrorType.Add))
                {
                    ret.AddRange(ret.ToArray().Select(t =>
                    {
                        var m = t.Copy();
                        m.Rotation = new Vector3D(t.Rotation.X - 180, t.Rotation.Y, t.Rotation.Z - 180);
                        return m;
                    }));
                }
                else
                {
                    ret = ret.Select(t =>
                    {
                        var m = t.Copy();
                        m.Rotation = new Vector3D(t.Rotation.X, t.Rotation.Y, -t.Rotation.Z);
                        return m;
                    }).ToList();
                }
            }

            return ret;
        }

        public static Vector3D Random(this Tuple<Vector3D, Vector3D> pointConstraint, Random random)
        {
            var x = (pointConstraint.Item1.X - pointConstraint.Item2.X) * random.NextDouble() + pointConstraint.Item2.X;
            var y = (pointConstraint.Item1.Y - pointConstraint.Item2.Y) * random.NextDouble() + pointConstraint.Item2.Y;
            var z = (pointConstraint.Item1.Z - pointConstraint.Item2.Z) * random.NextDouble() + pointConstraint.Item2.Z;
            return new Vector3D(x, y, z);
        }

        public static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> keyValue, out TK key, out TV value)
        {
            key = keyValue.Key;
            value = keyValue.Value;
        }

        public static string[] ReadAllLines(this StreamReader reader)
        {
            var ret = new List<string>();
            while (!reader.EndOfStream)
            {
                ret.Add(reader.ReadLine());
            }

            return ret.ToArray();
        }

        public static bool Invariant(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(string)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static object Copy(this object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceEqualityComparer()));
        }
        private static object InternalCopy(object originalObject, IDictionary<object, object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, IReflect typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (var fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((object)original);
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }

    public static class ArrayExtensions
    {
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            var walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (var i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (var i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (var j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
