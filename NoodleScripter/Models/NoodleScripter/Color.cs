using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Color
    {
        public Tuple<System.Windows.Media.Color, double>[] Colors { get; set; }
        public ColorType Type { get; set; } = ColorType.None;
        public float Repetitions { get; set; }

        public Wall[] SetColor(Wall[] walls)
        {
            switch (Type)
            {
                case ColorType.Single:
                    foreach (var wall in walls)
                    {
                        wall.Color = Colors[0].Item1.GetVector();
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
                            walls[i].Color = System.Windows.Media.Color.FromArgb((byte) ca, (byte) cr, (byte) cg, (byte) cb).GetVector();
                            prevTuple = nextTuple;
                        }
                        else
                        {
                            walls[i].Color = curTuple.Item1.GetVector();
                        }
                    }
                    break;
                case ColorType.Flash:
                    for (var i = 0; i < walls.Length; i++)
                    {
                        walls[i].Color = Colors[i % Colors.Length].Item1.GetVector();
                    }
                    break;
                case ColorType.Rainbow:
                    for (int i = 0; i < walls.Length; i++)
                    {
                        var p = i / walls.Length * 2 * Math.PI * Repetitions;
                        var r = Math.Sin(p + 0D / 3D * Math.PI) / 2D + 0.5D;
                        var g = Math.Sin(p + 2D / 3D * Math.PI) / 2D + 0.5D;
                        var b = Math.Sin(p + 4D / 3D * Math.PI) / 2D + 0.5D;
                        walls[i].Color = new Vector3D(r, g, b);
                    }
                    break;
                default:
                    break;
            }

            return walls;
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
