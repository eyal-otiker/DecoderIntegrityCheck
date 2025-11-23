using System.Collections.Generic;

namespace DecoderLibrary
{
    public class PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType> where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
    {
        public Dictionary<string, IcdDataType> IcdItemsDictionary { get; set; }
        public Dictionary<string, int> FrameDictionary { get; set; }
        public Dictionary<string, int> ClientDictionary { get; set; }
        public List<byte> RawMessage { get; set; }
        public GetParametersType GetParametersItem { get; set; }
        public EncoderType Encoder { get; set; }
        public DecoderType Decoder { get; set; }

        public PullerBlockItem(Dictionary<string, IcdDataType> icdItemsDictionary, Dictionary<string, int> frameDictionary,
            Dictionary<string, int> clientDictionary, List<byte> rawMessage, GetParametersType getParametersItem,
            EncoderType encoder, DecoderType decoder)
        {
            this.IcdItemsDictionary = icdItemsDictionary;
            this.FrameDictionary = frameDictionary;
            this.ClientDictionary = clientDictionary;
            this.RawMessage = rawMessage;
            this.GetParametersItem = getParametersItem;
            this.Encoder = encoder;
            this.Decoder = decoder;
        }
    }
}
