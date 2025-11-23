using System;
using System.Collections.Generic;
using System.Text;

namespace DecoderLibrary
{
    public class CompriasionBlockItem<IcdDataType, GetParametersType> where GetParametersType : IIcdItemParameters<IcdDataType>
    {
        public Dictionary<string, int> DecodeServerFrameDictionary { get; set; }
        public Dictionary<string, int> ClientDictionary { get; set; }
        public GetParametersType GetParametersItem { get; set; }
        public Dictionary<string, int> FrameDictionary { get; set; }
        public Dictionary<string, IcdDataType> IcdItemsDictionary { get; set; }

        public CompriasionBlockItem(Dictionary<string, int>  decodeServerFrameDictionary, Dictionary<string, int> clientDictionary,
            GetParametersType getParametersItem, Dictionary<string, int> frameDictionary, Dictionary<string, IcdDataType> icdItemsDictionary)
        {
            this.DecodeServerFrameDictionary = decodeServerFrameDictionary;
            this.ClientDictionary = clientDictionary;
            this.GetParametersItem = getParametersItem;
            this.FrameDictionary = frameDictionary;
            this.IcdItemsDictionary = icdItemsDictionary;
        }
    }
}
