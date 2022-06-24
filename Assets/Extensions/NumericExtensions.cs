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
    }
}