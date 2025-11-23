using System.Collections.Generic;
using DecoderLibrary;

namespace DecoderExample
{
    public class DecodeFiberBox
    {
        private Dictionary<string, FiberBoxItem> _icdItems;
        private Dictionary<string, int> _dataItems;

        public DecodeFiberBox(Dictionary<string, FiberBoxItem> icdItems, Dictionary<string, int> dataItems)
        {
            this._icdItems = icdItems;
            this._dataItems = dataItems;
        }

        public Dictionary<string, int> Decode()
        {
            Dictionary<string, int> decodeFrame = new Dictionary<string, int>();

            List<string> listKeys = new List<string>(this._dataItems.Keys);
            foreach (string nameOfItem in listKeys)
                decodeFrame[nameOfItem] = this._dataItems[nameOfItem];

            return decodeFrame;
        }
    }
}
