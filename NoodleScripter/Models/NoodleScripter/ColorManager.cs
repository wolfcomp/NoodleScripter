using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NoodleScripter.Models.BeatSaber;

namespace NoodleScripter.Models.NoodleScripter
{
    [JsonConverter(typeof(ColorConverter))]
    public struct Color
    {
        public double A;
        public double R;
        public double G;
        public double B;

        public static Color FromArgb(double a, double r, double g, double b)
        {
            return new Color
            {
                A = Math.Round(a, 5),
                R = Math.Round(r, 5),
                G = Math.Round(g, 5),
                B = Math.Round(b, 5)
            };
        }

        public static Color FromRgb(double r, double g, double b)
        {
            return FromArgb(1, r, g, b);
        }
    }

    public class ColorManager
    {
        public Tuple<Color, double>[] Colors { get; set; }
        public ColorType Type { get; set; } = ColorType.None;
        public float Repetitions { get; set; }
        public Easings Easing { get; set; } = Easings.Linear;
        private static EventType[] LaserTypes = { EventType.LightBackTopLasers, EventType.LightBottomBackSideLasers, EventType.LightLeftLasers, EventType.LightRightLasers, EventType.LightTrackRingNeons };

        public Wall[] SetColor(Wall[] walls)
        {
            switch (Type)
            {
                case ColorType.Single:
                    foreach (var wall in walls)
                    {
                        wall.Color = Colors[0].Item1;
                    }
                    break;
                case ColorType.Gradient:
                    var curTuple = Colors[0];
                    var prevTuple = Colors[0];
                    var count = walls.Length;
                    for (int i = 0; i < count; i++)
                    {
                        var time = i / (count - 1f);
                        if (Math.Abs(time) > 0)
                        {
                            var nextTuple = Colors.First(t => t.Item2 >= time);
                            if (nextTuple.Item2 > prevTuple.Item2) curTuple = prevTuple;
                            var tupleTime = (time - curTuple.Item2) / (nextTuple.Item2 - curTuple.Item2);
                            var cr = (1 - tupleTime) * curTuple.Item1.R + tupleTime * nextTuple.Item1.R;
                            var cg = (1 - tupleTime) * curTuple.Item1.G + tupleTime * nextTuple.Item1.G;
                            var cb = (1 - tupleTime) * curTuple.Item1.B + tupleTime * nextTuple.Item1.B;
                            var ca = (1 - tupleTime) * curTuple.Item1.A + tupleTime * nextTuple.Item1.A;
                            walls[i].Color = Color.FromArgb(ca, cr, cg, cb);
                            prevTuple = nextTuple;
                        }
                        else
                        {
                            walls[i].Color = curTuple.Item1;
                        }
                    }
                    break;
                case ColorType.Flash:
                    for (var i = 0; i < walls.Length; i++)
                    {
                        walls[i].Color = Colors[i % Colors.Length].Item1;
                    }
                    break;
                case ColorType.Rainbow:
                    for (int i = 0; i < walls.Length; i++)
                    {
                        var p = i / walls.Length * 2 * Math.PI * Repetitions;
                        var r = Math.Sin(p + 0D / 3D * Math.PI) / 2D + 0.5D;
                        var g = Math.Sin(p + 2D / 3D * Math.PI) / 2D + 0.5D;
                        var b = Math.Sin(p + 4D / 3D * Math.PI) / 2D + 0.5D;
                        walls[i].Color = Color.FromRgb(r, g, b);
                    }
                    break;
                default:
                    break;
            }

            return walls;
        }

        public Event[] SetColor(Event[] events)
        {
            var colorEvents = events.Where(t => t.Value != LightType.Off && LaserTypes.Contains(t.Type)).ToArray();
            switch (Type)
            {
                case ColorType.Single:
                    foreach (var @event in colorEvents)
                    {
                        @event.Color = Colors[0].Item1;
                    }
                    break;
                case ColorType.Gradient:
                    var curTuple = Colors[0];
                    var prevTuple = Colors[0];
                    var count = colorEvents.Length;
                    for (int i = 0; i < count; i++)
                    {
                        var time = i / (count - 1f);
                        if (Math.Abs(time) > 0)
                        {
                            var nextTuple = Colors.First(t => t.Item2 >= time);
                            if (nextTuple.Item2 > prevTuple.Item2) curTuple = prevTuple;
                            var tupleTime = (time - curTuple.Item2) / (nextTuple.Item2 - curTuple.Item2);
                            var cr = Easing.GetValue(curTuple.Item1.R, nextTuple.Item1.R, tupleTime);
                            var cg = Easing.GetValue(curTuple.Item1.G, nextTuple.Item1.G, tupleTime);
                            var cb = Easing.GetValue(curTuple.Item1.B, nextTuple.Item1.B, tupleTime);
                            var ca = Easing.GetValue(curTuple.Item1.A, nextTuple.Item1.A, tupleTime);
                            colorEvents[i].Color = Color.FromArgb(ca, cr, cg, cb);
                            prevTuple = nextTuple;
                        }
                        else
                        {
                            colorEvents[i].Color = curTuple.Item1;
                        }
                    }
                    break;
                case ColorType.Flash:
                    for (var i = 0; i < colorEvents.Length; i++)
                    {
                        colorEvents[i].Color = Colors[i % Colors.Length].Item1;
                    }
                    break;
                case ColorType.Rainbow:
                    var length = colorEvents.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var p = i / length * 2 * Math.PI * Repetitions;
                        var r = Math.Sin(p + 0D / 3D * Math.PI) / 2D + 0.5D;
                        var g = Math.Sin(p + 2D / 3D * Math.PI) / 2D + 0.5D;
                        var b = Math.Sin(p + 4D / 3D * Math.PI) / 2D + 0.5D;
                        colorEvents[i].Color = Color.FromRgb(r, g, b);
                    }
                    break;
                default:
                    break;
            }

            return events;
        }
    }

    public enum ColorType
    {
        None,
        Single,
        Gradient,
        Flash,
        Rainbow
    }
}
