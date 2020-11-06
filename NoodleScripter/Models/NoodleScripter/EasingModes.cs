using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace NoodleScripter.Models.NoodleScripter
{
    public enum Easings
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo
    }

    public class StaticRotation : IRotationMode
    {
        public Vector3D Rotation { get; set; }
        public Vector3D LocalRotation { get; set; }

        public Wall[] GetValue(Wall[] walls)
        {
            foreach (var wall in walls)
            {
                wall.LocalRotation = LocalRotation;
                wall.Rotation = Rotation;
            }

            return walls;
        }
    }

    public class EaseRotation : IRotationMode
    {
        public Tuple<Vector3D, double>[] Rotations { get; set; }
        public Tuple<Vector3D, double>[] LocalRotations { get; set; }
        public Easings Easing { get; set; } = Easings.Linear;

        public Wall[] GetValue(Wall[] walls)
        {
            var count = walls.Length;

            var curRotation = Rotations[0];
            var curLocalRotation = LocalRotations[0];

            var prevRotation = Rotations[1];
            var prevLocalRotation = LocalRotations[1];

            var rotationEasingStore = new Vector3EasingStore
            {
                Easing = Easing,
                StartVector = curRotation.Item1,
                EndVector = Rotations[1].Item1
            };
            var localRotationEasingStore = new Vector3EasingStore
            {
                Easing = Easing,
                StartVector = curLocalRotation.Item1,
                EndVector = LocalRotations[1].Item1
            };

            for (var i = 0; i < count; i++)
            {
                var time = i / (count - 1f);

                var nextRotation = Rotations.First(t => t.Item2 >= time);
                if (nextRotation.Item2 > prevRotation.Item2)
                {
                    curRotation = prevRotation;
                    rotationEasingStore.NextPoint(nextRotation.Item1);
                }
                var rotationTime = Math.Abs(time) <= 0 ? time : (time - curRotation.Item2) / (nextRotation.Item2 - curRotation.Item2);

                var nextLocalRotation = LocalRotations.First(t => t.Item2 >= time);
                if (nextLocalRotation.Item2 > prevLocalRotation.Item2)
                {
                    curLocalRotation = prevLocalRotation;
                    localRotationEasingStore.NextPoint(nextLocalRotation.Item1);
                }
                var localRotationTime = Math.Abs(time) <= 0 ? time : (time - curLocalRotation.Item2) / (nextLocalRotation.Item2 - curLocalRotation.Item2);

                walls[i].Rotation = rotationEasingStore.GetValue(rotationTime);
                walls[i].LocalRotation = localRotationEasingStore.GetValue(localRotationTime);
                if (Math.Abs(time) > 0)
                {
                    prevRotation = nextRotation;
                    prevLocalRotation = nextLocalRotation;
                }
            }

            return walls;
        }
    }

    public class RandomRotation : IRotationMode
    {
        public Tuple<Vector3D, Vector3D> Rotation { get; set; }
        public Tuple<Vector3D, Vector3D> LocalRotation { get; set; }
        public Random Random { get; set; }

        public Wall[] GetValue(Wall[] walls)
        {
            foreach (var wall in walls)
            {
                wall.Rotation = Rotation.Random(Random);
                wall.LocalRotation = LocalRotation.Random(Random);
            }

            return walls;
        }
    }

    public class SwitchRotation : IRotationMode
    {
        public Vector3D[] Rotations { get; set; }
        public Vector3D[] LocalRotations { get; set; }

        public Wall[] GetValue(Wall[] walls)
        {
            for (var i = 0; i < walls.Length; i++)
            {
                walls[i].Rotation = Rotations[i % Rotations.Length];
                walls[i].LocalRotation = LocalRotations[i % LocalRotations.Length];
            }

            return walls;
        }
    }

    public class NoRotation : IRotationMode
    {
        public Wall[] GetValue(Wall[] walls)
        {
            return walls;
        }
    }
}
