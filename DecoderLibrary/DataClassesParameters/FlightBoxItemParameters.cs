using System;
using System.Collections.Generic;
using System.Text;

namespace DecoderLibrary
{
    public class FlightBoxItemParameters : IIcdItemParameters<FlightBoxItem>
    {
        public string NameOfItem(FlightBoxItem flightBoxItem)
        {
            return flightBoxItem.Name;
        }

        public int LocationOfItem(FlightBoxItem flightBoxItem)
        {
            return flightBoxItem.Location;
        }

        public int MinValueOfItem(FlightBoxItem flightBoxItem)
        {
            return flightBoxItem.Min;
        }

        public int MaxValueOfItem(FlightBoxItem flightBoxItem)
        {
            return flightBoxItem.Max;
        }

        public string MaskOfItem(FlightBoxItem flightBoxItem)
        {
            return flightBoxItem.Mask;
        }

        public string CorrValueOfItem(FlightBoxItem flightBoxItem)
        {
            return string.Empty;
        }

        public int LengthOfItem(FlightBoxItem flightBoxItem)
        {
            return int.Parse(flightBoxItem.Bit);
        }
    }
}
