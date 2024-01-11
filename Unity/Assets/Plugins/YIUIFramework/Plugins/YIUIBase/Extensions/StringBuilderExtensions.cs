using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace YIUIFramework
{
    /// <summary>
    /// <see cref="StringBuilder"/>.
    /// </summary>
    public static class StringBuilderExtensions
    {
        // These digits are here in a static array to support hex with simple, 
        // easily-understandable code. Since A-Z don't sit next to 0-9 in the 
        // ASCII table.
        private static readonly char[] Digits = new char[]
        {
            '0', '1', '2', '3', '4',
            '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F'
        };

        // Matches standard .NET formatting dp's
        private static readonly uint DefaultDecimalPlaces = 5;
        private static readonly char DefaultPadChar       = '0';

        /// <summary>
        /// Convert a given unsigned integer value to a string and concatenate 
        /// onto the StringBuilder. Any base value allowed.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            uint               uintVal,
            uint               padAmount,
            char               padChar,
            uint               baseVal)
        {
            Assert.IsTrue(padAmount >= 0);
            Assert.IsTrue(baseVal > 0 && baseVal <= 16);

            // Calculate length of integer when written out
            uint length     = 0;
            uint lengthCalc = uintVal;

            do
            {
                lengthCalc /= baseVal;
                ++length;
            } while (lengthCalc > 0);

            // Pad out space for writing.
            builder.Append(padChar, (int)Mathf.Max(padAmount, length));

            int strpos = builder.Length;

            // We're writing backwards, one character at a time.
            while (length > 0)
            {
                --strpos;

                // Lookup from static char array, to cover hex values too
                builder[strpos] = Digits[uintVal % baseVal];

                uintVal /= baseVal;
                --length;
            }

            return builder;
        }

        /// <summary>
        /// Convert a given unsigned integer value to a string and concatenate 
        /// onto the StringBuilder. Assume no padding and base ten.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder, uint uintVal)
        {
            builder.Concat(uintVal, 0, DefaultPadChar, 10);
            return builder;
        }

        /// <summary>
        /// Convert a given unsigned integer value to a string and concatenate 
        /// onto the StringBuilder. Assume base ten.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder, uint uintVal, uint padAmount)
        {
            builder.Concat(uintVal, padAmount, DefaultPadChar, 10);
            return builder;
        }

        /// <summary>
        /// Convert a given unsigned integer value to a string and concatenate 
        /// onto the StringBuilder. Assume base ten.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            uint               uintVal,
            uint               padAmount,
            char               padChar)
        {
            builder.Concat(uintVal, padAmount, padChar, 10);
            return builder;
        }

        /// <summary>
        /// Convert a given signed integer value to a string and concatenate 
        /// onto the StringBuilder. Any base value allowed.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            int                intVal,
            uint               padAmount,
            char               padChar,
            uint               baseVal)
        {
            Assert.IsTrue(padAmount >= 0);
            Assert.IsTrue(baseVal > 0 && baseVal <= 16);

            // Deal with negative numbers
            if (intVal < 0)
            {
                builder.Append('-');

                // This is to deal with Int32.MinValue
                uint uintVal = uint.MaxValue - ((uint)intVal) + 1;
                builder.Concat(uintVal, padAmount, padChar, baseVal);
            }
            else
            {
                builder.Concat((uint)intVal, padAmount, padChar, baseVal);
            }

            return builder;
        }

        /// <summary>
        /// Convert a given signed integer value to a string and concatenate 
        /// onto the StringBuilder. Assume no padding and base ten.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder, int intVal)
        {
            builder.Concat(intVal, 0, DefaultPadChar, 10);
            return builder;
        }

        /// <summary>
        /// Convert a given signed integer value to a string and concatenate 
        /// onto the StringBuilder. Assume base ten.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder, int intVal, uint padAmount)
        {
            builder.Concat(intVal, padAmount, DefaultPadChar, 10);
            return builder;
        }

        /// <summary>
        /// Convert a given signed integer value to a string and concatenate 
        /// onto the StringBuilder. Assume base ten.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            int                intVal,
            uint               padAmount,
            char               padChar)
        {
            builder.Concat(intVal, padAmount, padChar, 10);
            return builder;
        }

        /// <summary>
        /// Convert a given float value to a string and concatenate onto the 
        /// StringBuilder
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            float              floatVal,
            uint               decimalPlaces,
            uint               padAmount,
            char               padChar)
        {
            Assert.IsTrue(padAmount >= 0);

            if (decimalPlaces == 0)
            {
                // No decimal places, just round up and print it as an int

                // Agh, Math.Floor() just works on doubles/decimals. Don't want 
                // to cast! Let's do this the old-fashioned way.
                int intVal;
                if (floatVal >= 0.0f)
                {
                    // Round up
                    intVal = (int)(floatVal + 0.5f);
                }
                else
                {
                    // Round down for negative numbers
                    intVal = (int)(floatVal - 0.5f);
                }

                builder.Concat(intVal, padAmount, padChar, 10);
            }
            else
            {
                int intPart = (int)floatVal;

                // First part is easy, just cast to an integer
                builder.Concat(intPart, padAmount, padChar, 10);

                // Decimal point
                builder.Append('.');

                // Work out remainder we need to print after the d.p.
                float remainder = Mathf.Abs(floatVal - intPart);

                // Multiply up to become an int that we can print
                do
                {
                    remainder *= 10;
                    --decimalPlaces;
                } while (decimalPlaces > 0);

                // Round up. It's guaranteed to be a positive number, so no 
                // extra work required here.
                remainder += 0.5f;

                // All done, print that as an int!
                builder.Concat((uint)remainder, 0, '0', 10);
            }

            return builder;
        }

        /// <summary>
        /// Convert a given float value to a string and concatenate onto the 
        /// StringBuilder. Assumes five decimal places, and no padding.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder, float floatVal)
        {
            builder.Concat(
                floatVal, DefaultDecimalPlaces, 0, DefaultPadChar);
            return builder;
        }

        /// <summary>
        /// Convert a given float value to a string and concatenate onto the 
        /// StringBuilder. Assumes no padding.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            float              floatVal,
            uint               decimalPlaces)
        {
            builder.Concat(floatVal, decimalPlaces, 0, DefaultPadChar);
            return builder;
        }

        /// <summary>
        /// Convert a given float value to a string and concatenate onto the 
        /// StringBuilder.
        /// </summary>
        public static StringBuilder Concat(
            this StringBuilder builder,
            float              floatVal,
            uint               decimalPlaces,
            uint               padAmount)
        {
            builder.Concat(floatVal, decimalPlaces, padAmount, DefaultPadChar);
            return builder;
        }
    }
}