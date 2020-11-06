using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Noise : WallGenerator
    {
        public new int? Amount { get; set; }
        public bool AvoidCenter { get; set; } = true;

        public Vector3D TopLeft
        {
            get => box.Item1;
            set => box = new Tuple<Vector3D, Vector3D>(value, BottomRight);
        }

        public Vector3D BottomRight
        {
            get => box.Item2;
            set => box = new Tuple<Vector3D, Vector3D>(TopLeft, value);
        }

        private Tuple<Vector3D, Vector3D> box = new Tuple<Vector3D, Vector3D>(new Vector3D(), new Vector3D());
        private double Start => Math.Min(TopLeft.Z, BottomRight.Z);
        private double End => Math.Max(Math.Max(TopLeft.Z, BottomRight.Z), Start + 0.0000001);

        public override WallGenerator GetWallGenerator(WallGenerator wallGenerator)
        {
            Random = wallGenerator.Random;
            Beat = wallGenerator.Beat;
            return this;
        }

        public override IEnumerable<Wall> GenerateWalls()
        {
            var ret = new List<Wall>();
            Amount ??= (int)Math.Round(8 * Start - End);
            for (var i = 0; i < Amount; i++)
            {
                var vector = getNext();
                ret.Add(new Wall
                {
                    Bomb = Bomb,
                    Width = Width,
                    Height = Height,
                    StartTime = Start + (i / (double)Amount.Value * (End - Start)),
                    StartHeight = vector.Y,
                    StartRow = vector.X,
                    Duration = Duration,
                    NJS = NJS,
                    Offset = Offset,
                    Track = Track
                });
            }

            return ret.ToArray();
        }

        private Vector3D getNext()
        {
            while (true)
            {
                if (!AvoidCenter) return box.Random(Random);
                var vector = box.Random(Random);
                if ((vector.X < 2.0 && vector.X > -2.0) || (vector.Y < 3.0 && vector.Y > 0.5)) continue;
                return vector;
            }
        }
    }
}
