using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace NoodleScripter
{
    public static class CurveCalculations
    {
        public static Vector3D LinearPoint(Vector3D one, Vector3D two, double time)
        {
            var x = (1-time) * one.X + time * two.X;
            var y = (1-time) * one.Y + time * two.Y;
            var z = (1-time) * one.Z + time * two.Z;
            return new Vector3D(x, y, z);
        }

        public static Vector3D LinearPoint(double time, Vector3D[] points)
        {
            return LinearPoint(points.First(), points.Last(), time);
        }

        public static Vector3D BezierPoint(double time, Vector3D[] points)
        {
            var consolidatedPoints = new List<Vector3D>();
            for (int i = 0; i < points.Length - 1; i++)
            {
                consolidatedPoints.Add(LinearPoint(points[i], points[i+1], time));
            }

            while (consolidatedPoints.Count > 1)
            {
                var iterate = consolidatedPoints.ToArray();
                consolidatedPoints.Clear();
                for (var i = 0; i < iterate.Length - 1; i++)
                {
                    consolidatedPoints.Add(LinearPoint(iterate[i], iterate[i+1], time));
                }
            }

            return consolidatedPoints[0];
        }
    }
}
