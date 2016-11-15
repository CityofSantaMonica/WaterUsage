using System;

namespace CSM.WaterUsage.ETL
{
    static class StringExtensions
    {
        public static string SafeTrim(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            return input.Trim();
        }
    }
}
