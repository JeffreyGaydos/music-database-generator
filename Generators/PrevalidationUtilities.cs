using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public static class PVU
    {
        private static LoggingUtils _logger;

        public static void Init(LoggingUtils logger)
        {
            _logger = logger;
        }

        public static string PrevalidateStringTruncate(string input, int maxLength, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(input) && input.Length > maxLength)
            {
                _logger.GenerationLogWriteData($"String {fieldName} was truncated to {input.Substring(0, maxLength)} (was {input})");
                return input.Substring(0, maxLength);
            }
            return input;
        }

        public static decimal PrevalidateDoubleToDecimalCast(double input, string fieldName)
        {
            double maxDecimalAsDouble = (double)decimal.MaxValue;
            if(input > maxDecimalAsDouble)
            {
                _logger.GenerationLogWriteData($"Double {fieldName} was truncated to {decimal.MaxValue} due to decimal cast (was {input})");
                return decimal.MaxValue;
            }
            return (decimal)input;
        }

        public static int PrevalidateUnsignedIntToIntCast(uint input, string fieldName)
        {
            uint maxIntAsUInt = int.MaxValue;
            if(input > maxIntAsUInt)
            {
                _logger.GenerationLogWriteData($"Unsigned int {fieldName} was truncated to {int.MaxValue} due to int cast (was {input})");
                return int.MaxValue;
            }
            return (int)input;
        }
    }
}
