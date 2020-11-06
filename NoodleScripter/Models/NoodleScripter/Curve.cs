using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Curve : WallGenerator
    {
        public CurveType Type { get; set; }
        public Vector3D[] Points { get; set; }
        public Easings Easing
        {
            get => Store.Easing;
            set => Store.Easing = value;
        }

        private Vector3EasingStore Store { get; set; } = new Vector3EasingStore { Easing = Easings.Linear };
        private Vector3D prevPoint;
        private Vector3D curPoint;

        public override WallGenerator GetWallGenerator(WallGenerator wallGenerator)
        {
            Random = wallGenerator.Random;
            return this;
        }

        public override IEnumerable<Wall> GenerateWalls()
        {
            var ret = new List<Wall>();
            prevPoint = Points[1];
            Store.StartVector = Points[0];
            Store.EndVector = prevPoint;
            for (int i = 0; i < Amount; i++)
            {
                var currentPoint = getPoint((double)i / Amount);
                var nextPoint = getPoint((i + 1D) / Amount);
                ret.Add(new Wall
                {
                    Bomb = Bomb,
                    Width = Math.Max(Width + (nextPoint.X - currentPoint.X), Width),
                    Height = Math.Max(Height + (nextPoint.Y - currentPoint.Y), Height),
                    StartTime = StartTime + Math.Min(currentPoint.Z, nextPoint.Z),
                    StartHeight = currentPoint.Y,
                    StartRow = currentPoint.X,
                    Duration = Duration < 0 ? Duration : Math.Abs(nextPoint.Z - currentPoint.Z),
                    NJS = NJS,
                    Offset = Offset,
                    Track = Track
                });
            }

            return ret;
        }

        private Vector3D getPoint(double time)
        {
            switch (Type)
            {
                case CurveType.Bezier:
                    return CurveCalculations.BezierPoint(time, Points);
                case CurveType.Easing:
                    var endPointTime = time * Points.Last().Z;
                    var nextPoint = Points.First(t => t.Z >= endPointTime);
                    if (nextPoint.Z > prevPoint.Z)
                    {
                        curPoint = prevPoint;
                        Store.NextPoint(nextPoint);
                    }
                    var pointTime = Math.Abs(endPointTime) <= 0 ? endPointTime : (endPointTime - curPoint.Z) / (nextPoint.Z - curPoint.Z);
                    
                    if (Math.Abs(time) > 0) prevPoint = nextPoint;
                    return Store.GetValue(pointTime);
                    //return CurveCalculations.LinearPoint(time, Points);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum CurveType
    {
        Bezier,
        Easing
    }
}
