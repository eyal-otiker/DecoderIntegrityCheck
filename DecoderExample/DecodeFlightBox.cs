using System;
using System.Collections.Generic;
using System.Linq;
using DecoderLibrary;

namespace DecoderExample
{
    public class DecodeFlightBox
    {
        private Dictionary<string, FlightBoxItem> _icdItems;
        private Dictionary<string, int> _dataItems;
        private Random _random;
        private int _currentLocation;
        private List<byte> _rawMessage;

        public DecodeFlightBox(Dictionary<string, FlightBoxItem> icdItems, Dictionary<string, int> dataItems)
        {
            this._icdItems = icdItems;
            this._dataItems = dataItems;
            this._random = new Random();
            this._currentLocation = 0;
            this._rawMessage = new List<byte>();
        }

        public Dictionary<string, int> Decode()
        {
            BuildRawMessage();
            return DecodeToFrame();
        }

        private void BuildRawMessage()
        {
            List<byte> bytesStringLine = new List<byte>(); FlightBoxItem icdItem;

            foreach (string nameOfIcdItem in this._icdItems.Keys)
            {
                icdItem = this._icdItems[nameOfIcdItem];

                if (icdItem.Location != -1 && this._dataItems.ContainsKey(nameOfIcdItem))
                {
                    if (icdItem.Location != this._currentLocation)
                        bytesStringLine = UpdateParametersInNewLine(bytesStringLine, icdItem);

                    List<byte> byteStringIcdItem = EncodeIcdItem(this._dataItems[nameOfIcdItem], int.Parse(icdItem.Bit));
                    if (byteStringIcdItem.Count != 0)
                        bytesStringLine.AddRange(byteStringIcdItem);
                }
            }

            this._rawMessage.AddRange(ReverseByteLine(bytesStringLine));
        }

        private List<byte> UpdateParametersInNewLine(List<byte> bytesStringLine, FlightBoxItem icdItem)
        {
            if (bytesStringLine != null)
                this._rawMessage.AddRange(ReverseByteLine(bytesStringLine));

            while (this._rawMessage.Count < icdItem.Location * 8)
                this._rawMessage.Add(2);

            this._currentLocation = (int)Math.Ceiling((double)this._rawMessage.Count / 8);

            return bytesStringLine;
        }

        public static List<byte> EncodeIcdItem(int num, int maskLength)
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

        private List<byte> ReverseByteLine(List<byte> bytesStringLine)
        {
            while (bytesStringLine.Count < 8)
                bytesStringLine.Add(0);

            return Enumerable.Reverse(bytesStringLine).ToList();
        }

        public Dictionary<string, int> DecodeToFrame()
        {
            Dictionary<string, int> decodeFrameDictionary = new Dictionary<string, int>();
            int icdItemValue; 

            foreach (string nameOfIcdItem in this._icdItems.Keys)
            {
                if (this._icdItems[nameOfIcdItem].Location != -1)
                {
                    try
                    {
                        icdItemValue = int.Parse(DecodeToFrame(this._icdItems[nameOfIcdItem], this._rawMessage));
                        decodeFrameDictionary.Add(nameOfIcdItem, icdItemValue);
                    }
                    catch (System.FormatException) { }
                }
            }

            return decodeFrameDictionary;
        }

        public string DecodeToFrame(FlightBoxItem flightBoxItem, List<byte> rawMessage)
        {
            List<byte> rawValueString = GetRawMessageInLocation(flightBoxItem, rawMessage);
            int rawValue = ConvertByteToNumber(rawValueString);
            string rawValueWithMask = ChangeValueWithMask(flightBoxItem, rawValue);
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
                int maskByte = ConvertByteToNumber(flightBoxItem.Mask);
                int andResultValue = maskByte & rawValue;

                andResultValue >>= int.Parse(flightBoxItem.StartBit.Split('-')[0]) - 1;
                rawValue = andResultValue;
            }

            return rawValue.ToString();
        }

        private int ConvertByteToNumber(string stringValue)
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

        private int ConvertByteToNumber(List<byte> stringValue, bool canValueBeNegative = false)
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
    }
}
