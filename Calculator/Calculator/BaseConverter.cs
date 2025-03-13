using System;
using System.Text;

namespace Calculator
{
    public class BaseConverter
    {
        private const string HexChars = "0123456789ABCDEF";

        public string ToBaseOld(long value, int toBase)
        {
            if (toBase < 2 || toBase > 16)
                throw new ArgumentOutOfRangeException(nameof(toBase), "Base must be between 2 and 16");

            // Handle negative values
            bool isNegative = value < 0;
            value = Math.Abs(value);

            StringBuilder result = new StringBuilder();

            // Convert to the target base
            do
            {
                int remainder = (int)(value % toBase);
                result.Insert(0, HexChars[remainder]);
                value /= toBase;
            } while (value > 0);

            // Add negative sign if necessary
            if (isNegative)
                result.Insert(0, "-");

            // Format for readability based on the base
            switch (toBase)
            {
                case 2: // Binary
                    // Group by 4 bits with spaces
                    for (int i = result.Length - 4; i > 0; i -= 4)
                        result.Insert(i, " ");
                    break;
                case 16: // Hex
                    // Add "0x" prefix
                    result.Insert(0, "0x");
                    break;
                case 8: // Octal
                    // Add "0" prefix for octal
                    result.Insert(0, "0");
                    break;
            }

            return result.ToString();
        }

        public string ToBase(long value, int toBase)
        {
            switch(toBase)
            {
                case 2:
                    string binary = Convert.ToString(value, 2);

                    StringBuilder formattedBinary = new StringBuilder();
                    for(int i=0; i < binary.Length; i++)
                    {
                        if (i > 0 && i % 4 == 0)
                            formattedBinary.Append(' ');
                        formattedBinary.Append(binary[i]);
                    }
                    return formattedBinary.ToString();
                case 8:
                    return "0" + Convert.ToString(value, 8);
                case 10:
                    return value.ToString();
                case 16:
                    return "0x" + Convert.ToString(value, 16).ToUpper();
                default:
                    throw new ArgumentOutOfRangeException(nameof(toBase), "Base must be 2, 8, 10 or 16");
            }
        }

        public long FromBaseOld(string value, int fromBase)
        {
            if (fromBase < 2 || fromBase > 16)
                throw new ArgumentOutOfRangeException(nameof(fromBase), "Base must be between 2 and 16");

            // Remove any formatting characters
            string cleanValue = value
                .Replace(" ", "")
                .Replace("0x", "")
                .ToUpper();

            bool isNegative = cleanValue.StartsWith("-");
            if (isNegative)
                cleanValue = cleanValue.Substring(1);

            // Convert from the specified base to decimal
            long result = 0;
            foreach (char c in cleanValue)
            {
                int digit;
                if (c >= '0' && c <= '9')
                    digit = c - '0';
                else if (c >= 'A' && c <= 'F')
                    digit = c - 'A' + 10;
                else
                    throw new ArgumentException("Invalid character in the input string");

                if (digit >= fromBase)
                    throw new ArgumentException($"Digit {c} is not valid in base {fromBase}");

                result = result * fromBase + digit;
            }

            return isNegative ? -result : result;
        }

        public long FromBase(string value, int fromBase)
        {
            // Clean up the input string based on the base
            string cleanValue = value.Trim();

            switch(fromBase)
            {
                case 2:
                    cleanValue = cleanValue.Replace(" ", "");
                    return Convert.ToInt64(cleanValue, 2);
                case 8:
                    if(cleanValue.StartsWith("0"))
                        cleanValue = cleanValue.Substring(1);
                    return Convert.ToInt64(cleanValue, 8);
                case 10:
                    return Convert.ToInt64(cleanValue, 10);
                case 16:
                    if(cleanValue.StartsWith("0x") || cleanValue.StartsWith("0X"))
                        cleanValue = cleanValue.Substring(2);
                    return Convert.ToInt64(cleanValue, 16);
                default:
                    throw new ArgumentOutOfRangeException(nameof(fromBase), "Base must be 2, 8, 10 or 16");
            }
        }
    }
}
