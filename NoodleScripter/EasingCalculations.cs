using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoodleScripter
{
    public static class EasingCalculations
    {
        private const double n1 = 7.5625;
        private const double d1 = 2.75;
        private const double c1 = 1.70158;
        private const double c2 = c1 * 1.525;
        private const double c3 = c1 + 1;
        private const double c4 = (2 * Math.PI) / 3;
        private const double c5 = (2 * Math.PI) / 4.5;

        public static double Linear(double x) => x;
        public static double EaseInQuad(double x) => x * x;
        public static double EaseOutQuad(double x) => x * (2 - x);
        public static double EaseInOutQuad(double x) => x < 0.5 ? 2 * x * x : -1 + (4 - 2 * x) * x;
        public static double EaseInCubic(double x) => x * x * x;
        public static double EaseOutCubic(double x) => Math.Pow(x - 1, 3) + 1;
        public static double EaseInOutCubic(double x) => x < 0.5 ? 4 * x * x * x : (x - 1) * (2 * x - 2) * (2 * x - 2) + 1;
        public static double EaseInQuart(double x) => x * x * x * x;
        public static double EaseOutQuart(double x) => 1 - Math.Pow(x - 1, 4);
        public static double EaseInOutQuart(double x) => x < 0.5 ? 8 * Math.Pow(x, 4) : 1 - 8 * Math.Pow(x - 1, 4);
        public static double EaseInQuint(double x) => Math.Pow(x, 5);
        public static double EaseOutQuint(double x) => 1 + Math.Pow(x - 1, 5);
        public static double EaseInOutQuint(double x) => x < 0.5 ? 16 * Math.Pow(x, 5) : 1 + 16 * Math.Pow(x - 1, 5);
        public static double EaseInSine(double x) => 1 - Math.Cos(x * Math.PI / 2);
        public static double EaseOutSine(double x) => Math.Sin(x * Math.PI / 2);
        public static double EaseInOutSine(double x) => -(Math.Cos(Math.PI * x) - 1) / 2;
        public static double EaseInCirc(double x) => 1 - Math.Sqrt(1 - Math.Pow(x, 2));
        public static double EaseOutCirc(double x) => Math.Sqrt(1 - Math.Pow(x - 1, 2));
        public static double EaseInOutCirc(double x) => x < 0.5 ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2 : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2;
        public static double EaseInBack(double x) => c3 * x * x * x - c1 * x * x;
        public static double EaseOutBack(double x) => 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
        public static double EaseInOutBack(double x) => x < 0.5 ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2 : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        public static double EaseInElastic(double x) => x switch
        {
            0.0 => 0.0,
            1.0 => 1.0,
            _ => -(Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * c4))
        };
        public static double EaseOutElastic(double x) => x switch
        {
            0.0 => 0.0,
            1.0 => 1.0,
            _ => -(Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1)
        };
        public static double EaseInOutElastic(double x) => 
            x == 0.0 ? 0.0 :
            x == 1.0 ? 1.0 :
            x < 0.5 ? -(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 :
            (Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 + 1;
        public static double EaseInBounce(double x) => 1 - bounceOut(x);
        public static double EaseOutBounce(double x) => bounceOut(x);
        public static double EaseInOutBounce(double x) => x < 0.5 ? (1 - bounceOut(1 - 2 * x)) / 2 : (1 + bounceOut(2 * x - 1)) / 2;

        private static double bounceOut(double x)
        {
            return 
                x < 1 / d1 ? n1 * x * x : x < 2 / d1 ? n1 * ((x - 1.5) / d1) * (x - 1.5) + 0.75 :
                x < 2.5 / d1 ? n1 * ((x - 2.25) / d1) * (x - 2.25) + 0.9375 :
                n1 * ((x - 2.625) / d1) * (x - 2.625) + 0.984375;
        }
    }
}
