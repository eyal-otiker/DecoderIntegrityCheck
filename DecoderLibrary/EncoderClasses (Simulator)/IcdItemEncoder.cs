using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoderLibrary
{
    public class IcdItemEncoder<IcdDataType, GetParametersType, EncoderType> where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType>
    {
        private readonly GetParametersType _itemGetParameters;
        private readonly EncoderType _itemEncoder;
        private readonly Random _random;
        private readonly List<byte> _rawMessage;
        private int _currentLocation;
        private int _correlatorValue;

        public IcdItemEncoder(GetParametersType icdItemGetParameters, EncoderType icdItemEncoder)
        {
            this._itemGetParameters = icdItemGetParameters;
            this._itemEncoder = icdItemEncoder;
            this._random = new Random();
            this._rawMessage = new List<byte>();
            this._currentLocation = 0;
            this._correlatorValue = -1;
        }

        /// <summary>
        /// builds raw message from ICD items dictionary or from frame dictionary 
        /// </summary>
        /// <param name="icdItemsDictionary"></param>
        /// <param name="rndMinMax"></param>
        /// <param name="frameDictionary"></param>
        /// <returns></returns>
        public List<byte> EncodeToRawMessage(Dictionary<string, IcdDataType> icdItemsDictionary, Dictionary<string, int> frameDictionary = null)
        {
            List<byte> bytesStringLine = new List<byte>(); IcdDataType icdItem;

            foreach (string nameOfIcdItem in icdItemsDictionary.Keys)
            {
                icdItem = icdItemsDictionary[nameOfIcdItem];

                if (this._itemGetParameters.LocationOfItem(icdItem) != -1 && frameDictionary.ContainsKey(nameOfIcdItem))
                {
                    if (this._itemGetParameters.LocationOfItem(icdItem) != this._currentLocation)
                        bytesStringLine = UpdateParametersInNewLine(bytesStringLine, icdItem);

                    List<byte> byteStringIcdItem = EncodeIcdItem(icdItemsDictionary, nameOfIcdItem, frameDictionary);
                    if (byteStringIcdItem.Count != 0)
                        bytesStringLine.AddRange(byteStringIcdItem);
                    UpdateCorrelatorValue(nameOfIcdItem, byteStringIcdItem);
                }
                else if (this._correlatorValue != -1 && this._itemGetParameters.CorrValueOfItem(icdItem) != string.Empty &&
                    ConvertingClass.ConvertCorrelateToNumber(this._itemGetParameters.CorrValueOfItem(icdItem)) == this._correlatorValue)
                    bytesStringLine = UpdateByteLineInIncorrectCorr(bytesStringLine, icdItem);
            }

            this._rawMessage.AddRange(ReverseByteLine(bytesStringLine));
            return this._rawMessage;
        }

        private List<byte> EncodeIcdItem(Dictionary<string, IcdDataType> icdItemsDictionary, string nameOfIcdItem, Dictionary<string, int> frameDictionary = null)
        {
            List<byte> byteStringIcdItem;

            if (frameDictionary != null)
                byteStringIcdItem = this._itemEncoder.EncodeWithFrameDictioanry(icdItemsDictionary[nameOfIcdItem], frameDictionary[nameOfIcdItem], this._correlatorValue);
            else
                byteStringIcdItem = this._itemEncoder.EncodeWithRandomNumber(icdItemsDictionary[nameOfIcdItem], this._random, this._correlatorValue);

            return byteStringIcdItem;
        }

        private void UpdateCorrelatorValue(string nameOfIcdItem, List<byte> byteStringIcdItem)
        {
            List<byte> reverseByteString = ReverseByteLine(byteStringIcdItem);
            if (nameOfIcdItem.Contains("correlator"))
                this._correlatorValue = ConvertingClass.ConvertByteToNumber(reverseByteString);
        }

        private List<byte> ReverseByteLine(List<byte> bytesStringLine)
        {
            while (bytesStringLine.Count < 8)
                bytesStringLine.Add(0);

            return Enumerable.Reverse(bytesStringLine).ToList();
        }

        private List<byte> UpdateParametersInNewLine(List<byte> bytesStringLine, IcdDataType icdItem)
        {
            if (bytesStringLine != null)
                this._rawMessage.AddRange(ReverseByteLine(bytesStringLine));

            while (this._rawMessage.Count < this._itemGetParameters.LocationOfItem(icdItem) * 8)
                this._rawMessage.Add(2);

            bytesStringLine = FixByteLineWithMask(this._itemGetParameters.MaskOfItem(icdItem));
            this._currentLocation = (int)Math.Ceiling((double)this._rawMessage.Count / 8);

            return bytesStringLine;
        }

        /// <summary>
        /// if the right bit in the mask is 0, this function adds the 0
        /// </summary>
        /// <returns></returns>
        private List<byte> FixByteLineWithMask(string maskItem)
        {
            List<byte> maskZeroStr = new List<byte>();
            int i = 1;

            if (maskItem != string.Empty)
            {
                while ((maskItem.Length != 8 && maskItem[maskItem.Length - i - 1] == '0') ||
                    (maskItem.Length == 8 && maskItem[maskItem.Length - i] == '0'))
                {
                    maskZeroStr.Add(0);
                    i++;
                }
            }

            return maskZeroStr;
        }

        private List<byte> UpdateByteLineInIncorrectCorr(List<byte> bytesStringLine, IcdDataType icdItem)
        {
            if (!bytesStringLine.Contains(2))
                bytesStringLine = UpdateParametersInNewLine(bytesStringLine, icdItem);

            for (int i = 0; i < this._itemGetParameters.LengthOfItem(icdItem); i++)
                bytesStringLine.Add(2);

            return bytesStringLine;
        }
    }
}
