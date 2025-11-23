using System.Collections.Generic;

namespace DecoderLibrary
{
    public class RawMessageDecoder<IcdDataType, GetPrametersType, EncoderType, DecoderType> where GetPrametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
    {
        private readonly GetPrametersType _icdItemGetParameters;
        private readonly EncoderType _icdItemEncoder;
        private readonly DecoderType _icdItemDecoder;

        public RawMessageDecoder(GetPrametersType icdItemGetParameters, EncoderType icdItemEncoder, DecoderType icdItemDecoder)
        {
            this._icdItemGetParameters = icdItemGetParameters;
            this._icdItemEncoder = icdItemEncoder;
            this._icdItemDecoder = icdItemDecoder;
        }

        public Dictionary<string, int> DecodeToFrame(Dictionary<string, IcdDataType> icdItemsDictionary, List<byte> rawMessage)
        {
            Dictionary<string, int> decodeFrameDictionary = new Dictionary<string, int>();
            int icdItemValue; int correlatorValue = -1;

            foreach (string nameOfIcdItem in icdItemsDictionary.Keys)
            {
                if (this._icdItemGetParameters.LocationOfItem(icdItemsDictionary[nameOfIcdItem]) != -1)
                { 
                    try
                    {
                        icdItemValue = int.Parse(this._icdItemDecoder.DecodeToFrame(icdItemsDictionary[nameOfIcdItem], rawMessage, this._icdItemEncoder, correlatorValue));
                        decodeFrameDictionary.Add(nameOfIcdItem, icdItemValue);

                        if (nameOfIcdItem.Contains("correlator"))
                            correlatorValue = icdItemValue;
                    }
                    catch (System.FormatException) { }
                }          
            }

            return decodeFrameDictionary;
        }
    }
}