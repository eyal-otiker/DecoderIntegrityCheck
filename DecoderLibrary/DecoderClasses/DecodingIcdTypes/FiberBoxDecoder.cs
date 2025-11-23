using System;
using System.Collections.Generic;
using System.Text;

namespace DecoderLibrary
{
    public class FiberBoxDecoder : IRawMessageDecoder<FiberBoxItem, FiberBoxEncoder>
    {
        public string DecodeToFrame(FiberBoxItem fiberBoxItem, List<byte> rawMessage, FiberBoxEncoder fiberBoxEncoder, int correlatorValue)
        {
            if ((fiberBoxItem.CorrValue != string.Empty && ConvertingClass.ConvertCorrelateToNumber(fiberBoxItem.CorrValue) == correlatorValue) || fiberBoxItem.CorrValue == string.Empty)
            {
                List<byte> rawValueString = GetRawMessageInLocation(fiberBoxItem, rawMessage);
                int rawValue = ConvertRawValue(fiberBoxItem, rawValueString);
                string rawValueWithMask = ChangeValueWithMask(fiberBoxItem, rawValue);

                if (fiberBoxEncoder.ExceptionIcdItemList.Contains(fiberBoxItem.Identifier))
                    return "out of range";
                else
                    return rawValueWithMask;
            }

            return string.Empty;
        }

        public List<byte> GetRawMessageInLocation(FiberBoxItem fiberBoxItem, List<byte> rawMessage)
        {
            int finalIndex; List<byte> lineByteList = new List<byte>();

            if (fiberBoxItem.Mask.Length != 0)
                finalIndex = 8 * int.Parse(fiberBoxItem.Loc) + fiberBoxItem.Mask.Length - 2;
            else
                finalIndex = 8 * int.Parse(fiberBoxItem.Loc) + fiberBoxItem.Size;

            for (int i = 8 * int.Parse(fiberBoxItem.Loc); i < finalIndex; i++)
                lineByteList.Add(rawMessage.ToArray()[i]);

            return lineByteList;
        }

        private int ConvertRawValue(FiberBoxItem fiberBoxItem, List<byte> rawValueString)
        {
            if (fiberBoxItem.PhysicalLimitMin != string.Empty && int.Parse(fiberBoxItem.PhysicalLimitMin) < 0)
                return ConvertingClass.ConvertByteToNumber(rawValueString, true);
            else
                return ConvertingClass.ConvertByteToNumber(rawValueString);
        }

        public string ChangeValueWithMask(FiberBoxItem fiberBoxItem, int rawValue)
        {
            if (fiberBoxItem.Mask.Length != 0)
            {
                int maskByte = ConvertingClass.ConvertByteToNumber(fiberBoxItem.Mask);
                int andResultValue = maskByte & rawValue;

                andResultValue /= (int)Math.Pow(2, FindStartBitMask(maskByte));
                rawValue = andResultValue;
            }
            
            if (fiberBoxItem.PhysicalLimitMax != string.Empty && int.Parse(fiberBoxItem.PhysicalLimitMax) < rawValue) // when value can be negaitve
                rawValue -= (int)Math.Pow(2, fiberBoxItem.Size);

            return rawValue.ToString();
        }

        private int FindStartBitMask(int mask)
        {
            int count = 0;

            while (mask % 2 != 1)
            {
                mask /= 2;
                count++;
            }

            return count;
        }

    }
}