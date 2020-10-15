using System;
using System.Collections.Generic;
using System.Linq;

namespace NoodleScripter.Models.NoodleScripter
{
    public class WallGenerator : WallGenerator<WallGenerator>
    {
        public override WallGenerator GetWallGenerator(WallGenerator wallGenerator)
        {
            Random = wallGenerator.Random;
            return this;
        }

        public override IEnumerable<Wall> GenerateWalls()
        {
            return new[]
            {
                new Wall
                {
                    Bomb = Bomb,
                    Width = Width,
                    Height = Height,
                    StartTime = StartTime,
                    StartHeight = StartHeight,
                    StartRow = StartRow,
                    Duration = Duration,
                    NJS = NJS,
                    Offset = Offset,
                    Track = Track
                }
            };
        }
    }

    public class Structure : WallGenerator
    {
        public override WallGenerator GetWallGenerator(WallGenerator wallGenerator)
        {
            Random = wallGenerator.Random;
            return this;
        }

        public override IEnumerable<Wall> GenerateWalls()
        {
            return Structures.SelectMany(t => t.GetWallGenerator(this).GenerateWallsFinalized());
        }
    }

    public abstract class WallGenerator<T>
    {
        public List<WallGenerator> Structures { get; set; } = new List<WallGenerator>();
        public Random Random { get; set; }
        public bool Bomb { get; set; }
        public float Beat { get; set; }
        public Color Color { get; set; } = new Color();
        public IRotationMode RotationMode { get; set; } = new NoRotation();
        public MirrorPoint Mirror { get; set; } = MirrorPoint.None;
        public MirrorType MirrorType { get; set; } = MirrorType.None;
        public Reverse Reverse { get; set; } = Reverse.None;
        public float? NJS { get; set; }
        public float? Offset { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double StartHeight { get; set; }
        public double StartRow { get; set; }
        public double Duration { get; set; }
        public double StartTime { get; set; }
        public double ScaleWidth { get; set; } = 1;
        public double ScaleHeight { get; set; } = 1;
        public double ScaleStartHeight { get; set; } = 1;
        public double ScaleStartRow { get; set; } = 1;
        public double ScaleDuration { get; set; } = 1;
        public double ScaleStartTime { get; set; } = 1;
        public double AddWidth { get; set; } = 0;
        public double AddHeight { get; set; } = 0;
        public double AddStartHeight { get; set; } = 0;
        public double AddStartRow { get; set; } = 0;
        public double AddDuration { get; set; } = 0;
        public double AddStartTime { get; set; } = 0;
        public double? FitWidth { get; set; } = null;
        public double? FitHeight { get; set; } = null;
        public double? FitStartHeight { get; set; } = null;
        public double? FitStartRow { get; set; } = null;
        public double? FitDuration { get; set; } = null;
        public double? FitStartTime { get; set; } = null;
        public double Scale { get; set; } = 1;
        public string Track { get; set; }

        public abstract T GetWallGenerator(WallGenerator wallGenerator);

        public abstract IEnumerable<Wall> GenerateWalls();

        public IEnumerable<Wall> GenerateWallsFinalized()
        {
            if (RotationMode is RandomRotation randomRotation) randomRotation.Random = Random;
            return adjust(RotationMode.GetValue(Color.SetColor(GenerateWalls().OrderBy(t => t.StartTime).ToArray()))).MirrorGenerator(this);
        }

        private IEnumerable<Wall> adjust(IEnumerable<Wall> walls)
        {
            foreach (var wall in walls)
            {
                wall.Width = wall.Width * ScaleWidth + AddWidth;
                wall.Height = wall.Height * ScaleHeight + AddHeight;
                wall.StartHeight = wall.StartHeight * ScaleStartHeight + AddStartHeight;
                wall.StartRow = wall.StartRow * ScaleStartRow + AddStartRow;
                wall.Duration = wall.Duration * ScaleDuration + AddDuration;
                wall.StartTime = wall.StartTime * ScaleStartTime + AddStartTime;

                if (FitDuration.HasValue)
                {
                    wall.StartTime = (wall.StartTime + wall.Duration) - FitDuration.Value;
                    wall.Duration = FitDuration.Value;
                }

                if (FitStartTime.HasValue)
                {
                    wall.Duration = (wall.Duration + wall.StartTime) - FitStartTime.Value;
                    wall.StartTime = FitStartTime.Value;
                }

                if (FitHeight.HasValue)
                {
                    wall.StartHeight = (wall.StartHeight + wall.Height) - FitHeight.Value;
                    wall.Height = FitHeight.Value;
                }

                if (FitStartHeight.HasValue)
                {
                    wall.Height = (wall.Height + wall.StartHeight) - FitStartHeight.Value;
                    wall.StartHeight = FitStartHeight.Value;
                }

                if (FitStartRow.HasValue)
                {
                    wall.Width = (wall.Width + wall.StartRow) - FitStartRow.Value;
                    wall.StartRow = FitStartRow.Value;
                }

                if (FitWidth.HasValue)
                {
                    wall.StartRow = (wall.StartRow + wall.Width) - FitWidth.Value;
                    wall.Width = FitWidth.Value;
                }

                wall.StartTime *= Scale;
                if (Duration > 0)
                    wall.Duration *= Scale;

                if(wall.StartTime < Beat)
                    wall.StartTime += Beat;

                if (Duration < 0)
                {
                    wall.StartTime += Global.Instance.HJSDuration;
                }
            }

            return walls;
        }
    }

    [Flags]
    public enum MirrorType
    {
        None,
        Add
    }

    [Flags]
    public enum MirrorPoint
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4
    }

    [Flags]
    public enum Reverse
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4
    }
}
