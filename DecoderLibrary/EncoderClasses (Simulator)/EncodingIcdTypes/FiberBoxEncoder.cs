using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public class FiberBoxEncoder : IIcdItemEncoder<FiberBoxItem>
    {
        public List<string> ExceptionIcdItemList { get; set; }

        public FiberBoxEncoder()
        {
            this.ExceptionIcdItemList = new List<string>();
        }

        /// <summary>
        /// choose random number
        /// </summary>
        /// <param name="fiberBoxItem"></param>
        /// <param name="random"></param>
        /// <param name="correlatorValue"></param>
        /// <returns></returns>
        public List<byte> EncodeWithRandomNumber(FiberBoxItem fiberBoxItem, Random random, int correlatorValue)
        {
            int objectValue;
            if (fiberBoxItem.PhysicalLimitMin != string.Empty)
                objectValue = random.Next(int.Parse(fiberBoxItem.PhysicalLimitMin), int.Parse(fiberBoxItem.PhysicalLimitMax) + 1);
            else
                objectValue = random.Next(int.Parse(fiberBoxItem.InterfaceLimitMin), int.Parse(fiberBoxItem.InterfaceLimitMax) + 1);

            return EncodeWithFrameDictioanry(fiberBoxItem, objectValue, correlatorValue);
        }

        /// <summary>
        /// function that sends the number to function that convert string to number
        /// </summary>
        /// <param name="fiberBoxItem"></param>
        /// <param name="itemValue"></param>
        /// <param name="correlatorValue"></param>
        /// <returns></returns>
        public List<byte> EncodeWithFrameDictioanry(FiberBoxItem fiberBoxItem, int itemValue, int correlatorValue)
        {
            if (!CheckIfValueInRange(fiberBoxItem, itemValue))
                this.ExceptionIcdItemList.Add(fiberBoxItem.Identifier);

            if ((fiberBoxItem.CorrValue != string.Empty && ConvertingClass.ConvertCorrelateToNumber(fiberBoxItem.CorrValue) == correlatorValue) ||
                fiberBoxItem.CorrValue == string.Empty)
                return ConvertingClass.ConvertNumberToByte(itemValue, fiberBoxItem.Size);

            return null;
        }

        public bool CheckIfValueInRange(FiberBoxItem fiberBoxItem, int itemValue)
        {
            if (fiberBoxItem.PhysicalLimitMin != string.Empty)
            {
                if (int.Parse(fiberBoxItem.PhysicalLimitMin) > itemValue || int.Parse(fiberBoxItem.PhysicalLimitMax) < itemValue)
                    return false;
            }
            else
            {
                if (int.Parse(fiberBoxItem.InterfaceLimitMin) > itemValue || int.Parse(fiberBoxItem.InterfaceLimitMax) < itemValue)
                    return false;
            }

            return true;
        }
    }
}
