using System.Collections.Generic;

namespace DecoderLibrary
{
    public class FlightBoxDecoder : IRawMessageDecoder<FlightBoxItem, FlightBoxEncoder>
    {
        public string DecodeToFrame(FlightBoxItem flightBoxItem, List<byte> rawMessage, FlightBoxEncoder flightBoxEncoder, int correlator = -1)
        {
            List<byte> rawValueString = GetRawMessageInLocation(flightBoxItem, rawMessage);
            int rawValue = ConvertingClass.ConvertByteToNumber(rawValueString);
            string rawValueWithMask = ChangeValueWithMask(flightBoxItem, rawValue);

            if (flightBoxEncoder.ExceptionIcdItemList.Contains(flightBoxItem.Name))
                return "out of range";
            else
                return rawValueWithMask;
        }

        public List<byte> GetRawMessageInLocation(FlightBoxItem flightBoxItem, List<byte> rawMessage)
        {
            int finalIndex; List<byte> lineByteList = new List<byte>();

            if (flightBoxItem.Mask.Length != 0)
                finalIndex = 8 * flightBoxItem.Location + flightBoxItem.Mask.Length;
            else
                finalIndex = 8 * flightBoxItem.Location + int.Parse(flightBoxItem.Bit);

            for (int i = 8 * flightBoxItem.Location; i < finalIndex; i++)
                lineByteList.Add(rawMessage.ToArray()[i]);

            return lineByteList;
        }

        public string ChangeValueWithMask(FlightBoxItem flightBoxItem, int rawValue)
        {
            if (flightBoxItem.Mask.Length != 0)
            {
                int maskByte = ConvertingClass.ConvertByteToNumber(flightBoxItem.Mask);
                int andResultValue = maskByte & rawValue;

                andResultValue >>= int.Parse(flightBoxItem.StartBit.Split('-')[0]) - 1;
                rawValue = andResultValue;
            }

            return rawValue.ToString();
        }
    }
}