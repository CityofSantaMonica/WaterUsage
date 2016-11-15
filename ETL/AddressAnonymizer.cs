using System;

namespace CSM.WaterUsage.ETL
{
    class AddressAnonymizer
    {
        public static string Anonymize(decimal? number, string street)
        {
            if (!number.HasValue)
            {
                return street;
            }

            int intNumber = (int)number.Value;

            string format = "{0}BLK {1}";
            int result = (int)(Math.Round(number.Value / 100M, MidpointRounding.ToEven) * 100);

            if (result > intNumber)
                result -= 100;

            return String.Format(format, result, street);
        }
    }
}
