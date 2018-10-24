using System;
using System.Globalization;
using System.Text;

namespace GameStateManager
{
    // Options for StringBuilder extension methods.
    [Flags]
    public enum AppendNumberOptions
    {
        None = 0, // Normal format.
        PositiveSign = 1, // Added "+" sign for positive value.
        NumberGroup = 2, // Number group separation characters. In Use: "," for every 3 digits.
    }


    // Static class for string builder extension methods and avoid unwanted memory allocations.
    // All methods are defined as extension methods as StringBuilder.
    public static class StringBuilderExtensions
    {
        // Cache for NumberGroupSizes of NumberFormat class.
        private static int[] numberGroupSizes = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes;

        // String conversion buffer.
        private static char[] numberString = new char[32];


        // Convert integer to string and add to string builder.
        public static void AppendNumber(this StringBuilder builder, int number)
        {
            AppendNumberInternal(builder, number, 0, AppendNumberOptions.None);
        }


        // Convert integer to string and add to string builder.
        public static void AppendNumber(this StringBuilder builder, 
            int number, AppendNumberOptions options)
        {
            AppendNumberInternal(builder, number, 0, options);
        }


        // Convert float to string and add to string builder.
        public static void AppendNumber(this StringBuilder builder, float number)
        {
            AppendNumber(builder, number, 2, AppendNumberOptions.None);
        }


        // Convert float to string and add to string builder. It shows 2 decimal digits.
        public static void AppendNumber(this StringBuilder builder, 
            float number, AppendNumberOptions options)
        {
            AppendNumber(builder, number, 2, options);
        }


        // Convert float to string and add to string builder.
        public static void AppendNumber(this StringBuilder builder, 
            float number, int decimalCount, AppendNumberOptions options)
        {
            // Handle NaN, Infinity cases.
            if (float.IsNaN(number))
                builder.Append("NaN");
            else if (float.IsNegativeInfinity(number))
                builder.Append("-Infinity");
            else if (float.IsPositiveInfinity(number))
                builder.Append("+Infinity");
            else
            {
                int intNumber = (int)(number * (float)Math.Pow(10, decimalCount) + 0.5f);
                AppendNumberInternal(builder, intNumber, decimalCount, options);
            }
        }


        private static void AppendNumberInternal(StringBuilder builder, int number, 
            int decimalCount, AppendNumberOptions options)
        {
            // Initialize variables for conversion.
            NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;

            int idx = numberString.Length;
            int decimalPos = idx - decimalCount;

            if (decimalPos == idx)
                decimalPos = idx + 1;

            int numberGroupIdx = 0;
            int numberGroupCount = numberGroupSizes[numberGroupIdx] + decimalCount;

            bool showNumberGroup = (options & AppendNumberOptions.NumberGroup) != 0;
            bool showPositiveSign = (options & AppendNumberOptions.PositiveSign) != 0;

            bool isNegative = number < 0;
            number = Math.Abs(number);

            // Converting from smallest digit.
            do
            {
                // Add decimal separator ("." in US).
                if (idx == decimalPos)
                    numberString[--idx] = nfi.NumberDecimalSeparator[0];

                // Added number group separator ("," in US).
                if (--numberGroupCount < 0 && showNumberGroup)
                {
                    numberString[--idx] = nfi.NumberGroupSeparator[0];

                    if (numberGroupIdx < numberGroupSizes.Length - 1)
                        numberGroupIdx++;

                    numberGroupCount = numberGroupSizes[numberGroupIdx] - 1;
                }

                // Convert current digit to character and add to buffer.
                numberString[--idx] = (char)('0' + (number % 10));
                number /= 10;

            } while (number > 0 || decimalPos <= idx);


            // Added sign character if needed.
            if (isNegative)
                numberString[--idx] = nfi.NegativeSign[0];
            else if (showPositiveSign)
                numberString[--idx] = nfi.PositiveSign[0];

            // Added converted string to StringBuilder.
            builder.Append(numberString, idx, numberString.Length - idx);
        }
    }
}