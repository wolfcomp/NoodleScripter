using System;
using System.Windows;
using System.Windows.Media.Media3D;
using NoodleScripter.Models.NoodleScripter;

namespace NoodleScripter
{
    public class Vector3EasingStore : IEasings<Vector3D>
    {
        private readonly EasingStore _xStore = new EasingStore();
        private readonly EasingStore _yStore = new EasingStore();
        private readonly EasingStore _zStore = new EasingStore();

        public Vector3D StartVector
        {
            get => new Vector3D(_xStore.Start, _yStore.Start, _zStore.Start);
            set
            {
                _xStore.Start = value.X;
                _yStore.Start = value.Y;
                _zStore.Start = value.Z;
            }
        }

        public Vector3D EndVector
        {
            get => new Vector3D(_xStore.End, _yStore.End, _zStore.End);
            set
            {
                _xStore.End = value.X;
                _yStore.End = value.Y;
                _zStore.End = value.Z;
            }
        }

        public Easings Easing
        {
            get => _xStore.Easing;
            set
            {
                _xStore.Easing = value;
                _yStore.Easing = value;
                if(!LinearTimeScale) _zStore.Easing = value;
            }
        }

        public bool LinearTimeScale { get; set; } = true;

        public Vector3D GetValue(double time) => new Vector3D(_xStore.GetValue(time), _yStore.GetValue(time), _zStore.GetValue(time));

        public void NextPoint(Vector3D point)
        {
            StartVector = EndVector;
            EndVector = point;
        }
    }

    public class VectorEasingStore : IEasings<Vector>
    {
        private readonly EasingStore _xStore = new EasingStore();
        private readonly EasingStore _yStore = new EasingStore();

        public Vector StartVector
        {
            get => new Vector(_xStore.Start, _yStore.Start);
            set
            {
                _xStore.Start = value.X;
                _yStore.Start = value.Y;
            }
        }

        public Vector EndVector
        {
            get => new Vector(_xStore.End, _yStore.End);
            set
            {
                _xStore.End = value.X;
                _yStore.End = value.Y;
            }
        }

        public Easings Easing
        {
            get => _xStore.Easing;
            set
            {
                _xStore.Easing = value;
                _yStore.Easing = value;
            }
        }

        public Vector GetValue(double time) => new Vector(_xStore.GetValue(time), _yStore.GetValue(time));
        public void NextPoint(Vector point)
        {
            StartVector = EndVector;
            EndVector = point;
        }
    }

    public class EasingStore
    {
        public double Start { get; set; }
        public double End { get; set; }
        public Easings Easing { get; set; }

        public double GetValue(double time) => Start + (End - Start) * easing(time);

        private double easing(double time) => Easing switch
        {
            Easings.Linear => EasingCalculations.Linear(time),
            Easings.EaseInQuad => EasingCalculations.EaseInQuad(time),
            Easings.EaseOutQuad => EasingCalculations.EaseOutQuad(time),
            Easings.EaseInOutQuad => EasingCalculations.EaseInOutQuad(time),
            Easings.EaseInCubic => EasingCalculations.EaseInCubic(time),
            Easings.EaseOutCubic => EasingCalculations.EaseOutCubic(time),
            Easings.EaseInOutCubic => EasingCalculations.EaseInOutCubic(time),
            Easings.EaseInQuart => EasingCalculations.EaseInQuart(time),
            Easings.EaseOutQuart => EasingCalculations.EaseOutQuart(time),
            Easings.EaseInOutQuart => EasingCalculations.EaseInOutQuart(time),
            Easings.EaseInQuint => EasingCalculations.EaseInQuint(time),
            Easings.EaseOutQuint => EasingCalculations.EaseOutQuint(time),
            Easings.EaseInOutQuint => EasingCalculations.EaseInOutQuint(time),
            Easings.EaseInSine => EasingCalculations.EaseInSine(time),
            Easings.EaseOutSine => EasingCalculations.EaseOutSine(time),
            Easings.EaseInOutSine => EasingCalculations.EaseInOutSine(time),
            Easings.EaseInCirc => EasingCalculations.EaseInCirc(time),
            Easings.EaseOutCirc => EasingCalculations.EaseOutCirc(time),
            Easings.EaseInOutCirc => EasingCalculations.EaseInOutCirc(time),
            Easings.EaseInBack => EasingCalculations.EaseInBack(time),
            Easings.EaseOutBack => EasingCalculations.EaseOutBack(time),
            Easings.EaseInOutBack => EasingCalculations.EaseInOutBack(time),
            Easings.EaseInElastic => EasingCalculations.EaseInElastic(time),
            Easings.EaseOutElastic => EasingCalculations.EaseOutElastic(time),
            Easings.EaseInOutElastic => EasingCalculations.EaseInOutElastic(time),
            Easings.EaseInBounce => EasingCalculations.EaseInBounce(time),
            Easings.EaseOutBounce => EasingCalculations.EaseOutBounce(time),
            Easings.EaseInOutBounce => EasingCalculations.EaseInOutBounce(time),
            Easings.EaseInExpo => EasingCalculations.EaseInExpo(time),
            Easings.EaseOutExpo => EasingCalculations.EaseOutExpo(time),
            Easings.EaseInOutExpo => EasingCalculations.EaseInOutExpo(time),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public interface IEasings<T> where T : IFormattable
    {
        public T GetValue(double time);
        public void NextPoint(T point);
    }

    public interface IRotationMode
    {
        public Wall[] GetValue(Wall[] walls);
    }
}
