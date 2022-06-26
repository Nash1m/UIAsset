using UnityEngine;

namespace Nash1m.Extensions
{
    public static class NumericExtensions
    {
        public static float Half(this int value) => value / 2.0f;
        public static float Half(this float value) => value / 2.0f;

        public static float Normalized(this float value, float min, float max)
        {
            var diff = max - min;
            return value / diff;
        }

        public static float Normalized(this int value, int min, int max)
        {
            var diff = max - min;
            return (float) value / (float) diff;
        }

        public static float Normalized(this int value, float min, float max)
        {
            var diff = max - min;
            return value / diff;
        }

        public static float Percent(this int value, float percent)
        {
            return value * (percent / 100f);
        }

        public static float Percent(this float value, float percent)
        {
            return value * (percent / 100f);
        }

        public static float Factorial(int number)
        {
            if (number <= 1)
                return 1;
            else
                return number * Factorial(number - 1);
        }
        
        public static float Loop(this float value, float min, float max)
        {
            var range = max - min;
            var modular = value % range;
            return min + modular;
        }
        public static float PingPong(this float value, float min, float max)
        {
            var range = max - min;
            var count = Mathf.FloorToInt(value / range);
            var ascending = count % 2 == 0;
            var modular = value % range;
            return ascending ? modular + min : max - modular;
        }
    }
}