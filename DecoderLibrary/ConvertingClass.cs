using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    internal static class ConvertingClass
    {
        public static int ConvertCorrelateToNumber(string corrString)
        {
            string convertCorr = string.Empty;
            char[] corrCharArray = corrString.ToCharArray();
            
            for (int i = 1; i < corrCharArray.Length - 1; i++)
            {
                if (corrCharArray[i] != '0')
                    convertCorr += corrCharArray[i];
            }

            if (convertCorr != string.Empty)
                return int.Parse(convertCorr);
            else
                return 0;
        }
        
        public static int ConvertByteToNumber(List<byte> stringValue, bool canValueBeNegative = false)
        {
            int num = 0; int mult = 1; int firstIndexValue = -1;
   
            for (int i = stringValue.Count - 1; i >= 0; i--)
            {
                if (stringValue[i] == 0 || stringValue[i] == 1)
                {
                    firstIndexValue = i;
                    num += stringValue[i] * mult;
                    mult *= 2;
                }           
            }

            if (canValueBeNegative && firstIndexValue != -1 && stringValue[firstIndexValue] == '1')
                num -= (int)Math.Pow(2, stringValue.Count);

            return num;
        }

        public static int ConvertByteToNumber(string stringValue)
        {
            int num = 0; int mult = 1;

            for (int i = stringValue.Length - 1; i >= 0; i--)
            {
                if (stringValue[i] == '0' || stringValue[i] == '1')
                {
                    num += (stringValue[i] - '0') * mult;
                    mult *= 2;
                }
            }

            return num;
        }

        public static List<byte> ConvertNumberToByte(int num, int maskLength)
        {
            List<byte> byteNumber = new List<byte>();
            int count = 0;

            if (num < 0)
                num = (int)Math.Pow(2, maskLength) - Math.Abs(num);

            while (num != 0 || count < maskLength)
            {
                byteNumber.Add((byte)((byte)num % 2));
                num /= 2;
                count++;
            }

            return byteNumber;
        }
    }
}
