using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public class FlightBoxEncoder : IIcdItemEncoder<FlightBoxItem>
    {
        public List<string> ExceptionIcdItemList { get; set; }

        public FlightBoxEncoder()
        {
            this.ExceptionIcdItemList = new List<string>();
        }

        /// <summary>
        /// choose random number
        /// </summary>
        /// <param name="flightBoxItemEncoded"></param>
        /// <param name="rndMinMax"></param>
        /// <returns></returns>
        public List<byte> EncodeWithRandomNumber(FlightBoxItem flightBoxItem, Random rndMinMax, int correlatorValue = -1)
        {
            int objectValue = rndMinMax.Next(flightBoxItem.Min, flightBoxItem.Max + 1);
            return EncodeWithFrameDictioanry(flightBoxItem, objectValue);
        }

        /// <summary>
        /// function that sends the number to function that convert string to number
        /// </summary>
        /// <param name="flightBoxItemEncoded"></param>
        /// <param name="itemValue"></param>
        /// <returns></returns>
        public List<byte> EncodeWithFrameDictioanry(FlightBoxItem flightBoxItem, int itemValue, int correlatorValue = -1)
        {
            if (!CheckIfValueInRange(flightBoxItem, itemValue))
                this.ExceptionIcdItemList.Add(flightBoxItem.Name);

            return ConvertingClass.ConvertNumberToByte(itemValue, int.Parse(flightBoxItem.Bit));
        }

        public bool CheckIfValueInRange(FlightBoxItem flightBoxItem, int value)
        {
            if (flightBoxItem.Min > value || flightBoxItem.Max < value)
                return false;
            return true;
        }
    }
}
