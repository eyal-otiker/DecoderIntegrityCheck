using Newtonsoft.Json;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public class JsonToIcdItemConverter<IcdDataType, GetParametersType> where GetParametersType : IIcdItemParameters<IcdDataType>
    {
        private string _icdText;
        private GetParametersType _itemGetParameters;

        public JsonToIcdItemConverter(string icdText, GetParametersType convertsItem)
        {
            this._icdText = icdText;
            this._itemGetParameters = convertsItem;
        }

        public Dictionary<string, IcdDataType> ConverterManager()
        {
            List<IcdDataType> icdItemsList = ConvertJsonToIcdItemList();
            Dictionary<string, IcdDataType> icdItemsDictioanry = ConvertListIcdItemsToDictionary(icdItemsList);
            return icdItemsDictioanry;
        }

        /// <summary>
        /// function that do deserealize to the text of the ICD file
        /// </summary>
        /// <param name="icdText"></param>
        /// <returns></returns>
        private List<IcdDataType> ConvertJsonToIcdItemList()
        {
            List<IcdDataType> listItems = JsonConvert.DeserializeObject<List<IcdDataType>>(this._icdText);
            return listItems;
        }

        /// <summary>
        /// convert the list of type T (the type of the ICD item) to dictionary of type U (the encoded of T type)
        /// </summary>
        /// <param name="listOriginalItems"></param>
        /// <returns></returns>
        private Dictionary<string, IcdDataType> ConvertListIcdItemsToDictionary(List<IcdDataType> list)
        {
            Dictionary<string, IcdDataType> dictionaryIcdItems = new Dictionary<string, IcdDataType>();

            foreach (IcdDataType originalItem in list)
            {
                string itemName = this._itemGetParameters.NameOfItem(originalItem);
                if (!dictionaryIcdItems.ContainsKey(itemName))
                    dictionaryIcdItems.Add(itemName, originalItem);
            }
                              
            return dictionaryIcdItems;
        }
    }
}
