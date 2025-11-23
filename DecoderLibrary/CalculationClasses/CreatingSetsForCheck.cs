using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public class CreatingSetsForCheck<IcdDataType, GetParametersType> where GetParametersType : IIcdItemParameters<IcdDataType>
    {
        private readonly GetParametersType _itemGetParmeters;
        private readonly Dictionary<string, IcdDataType> _icdItemsDictionary;
        private readonly string[] _itemsNameArr;
        private int _currentIndex;
        private readonly string _nativeCheck;
        private readonly Random _random;

        public CreatingSetsForCheck(GetParametersType icdItemGetParmeters, Dictionary<string, IcdDataType> icdItemsDictionary, string nativeCheck)
        {
            this._itemGetParmeters = icdItemGetParmeters;
            this._icdItemsDictionary = icdItemsDictionary;
            this._itemsNameArr = new string[this._icdItemsDictionary.Count];
            BuildItemsNamesArr();
            this._currentIndex = this._itemsNameArr.Length - 1;
            this._nativeCheck = nativeCheck;
            this._random = new Random();
        }

        private Dictionary<string, int> SetStartDictionary(bool isRandomCheck)
        {
            Dictionary<string, int> frameDictionary = new Dictionary<string, int>();
            int updateCorrlator = -1; string correlateString;

            foreach (string nameOfItem in this._icdItemsDictionary.Keys)
            {
                correlateString = this._itemGetParmeters.CorrValueOfItem(this._icdItemsDictionary[nameOfItem]);

                if ((correlateString != string.Empty && ConvertingClass.ConvertCorrelateToNumber(correlateString) == updateCorrlator) 
                    || correlateString == string.Empty)
                {
                    if (isRandomCheck == false)
                        frameDictionary[nameOfItem] = this._itemGetParmeters.MinValueOfItem(this._icdItemsDictionary[nameOfItem]);
                    else
                        frameDictionary[nameOfItem] = this._random.Next(this._itemGetParmeters.MinValueOfItem(this._icdItemsDictionary[nameOfItem]),
                            this._itemGetParmeters.MaxValueOfItem(this._icdItemsDictionary[nameOfItem]) + 1);
                }

                if (nameOfItem.Contains("correlator"))
                    updateCorrlator = frameDictionary[nameOfItem];
            }

            return frameDictionary;
        }

        private void BuildItemsNamesArr()
        {
            int i = 0;

            foreach (string nameOfItem in this._icdItemsDictionary.Keys)
            {
                this._itemsNameArr[i] = nameOfItem;
                i++;
            }

        }

        public Dictionary<string, int> CreateFrameDictionaryManager(bool isRandomCheck, Dictionary<string, int> frameDictionary = null)
        {
            if (frameDictionary == null && isRandomCheck == false)
                return SetStartDictionary(false);
            else if (isRandomCheck == true)
                return SetStartDictionary(true);
            else
                return CreateFrameDictionaryInSerialCheck(frameDictionary);
        }

        private Dictionary<string, int> CreateFrameDictionaryInSerialCheck(Dictionary<string, int> frameDictionary)
        {
            int firstIndexNotInMaxValue = FirstValueItemNotInMax(frameDictionary);

            if (firstIndexNotInMaxValue == -1)
                return null;
            else
            {
                string nameOfCurrentItem = this._itemsNameArr[firstIndexNotInMaxValue];

                if (firstIndexNotInMaxValue + (this._itemsNameArr.Length - frameDictionary.Count) < this._currentIndex)
                {
                    frameDictionary = SetStartDictionary(false);
                    this._currentIndex = firstIndexNotInMaxValue;
                }

                frameDictionary = SetValueWithNativeCheck(nameOfCurrentItem, frameDictionary);
                if (nameOfCurrentItem.Contains("correlator"))
                    frameDictionary = FixFrameDictioanryWithCorrelater(firstIndexNotInMaxValue, frameDictionary);
            }

            return frameDictionary;
        }

        private int FirstValueItemNotInMax(Dictionary<string, int> frameDictionary)
        {
            int index = this._itemsNameArr.Length - 1;
            string nameOfCurrentItem;

            while (index >= 0)
            {
                nameOfCurrentItem = this._itemsNameArr[index];

                if (frameDictionary.ContainsKey(nameOfCurrentItem))
                {
                    if (frameDictionary[nameOfCurrentItem] < this._itemGetParmeters.MaxValueOfItem(this._icdItemsDictionary[nameOfCurrentItem]))
                        return index;
                    else
                        frameDictionary[nameOfCurrentItem] = this._itemGetParmeters.MinValueOfItem(this._icdItemsDictionary[nameOfCurrentItem]);
                }               

                index--;
            }

            return index;
        }

        private Dictionary<string, int> SetValueWithNativeCheck(string itemName, Dictionary<string, int> frameDictionary)
        {
            int minValue = this._itemGetParmeters.MinValueOfItem(this._icdItemsDictionary[itemName]);
            int maxValue = this._itemGetParmeters.MaxValueOfItem(this._icdItemsDictionary[itemName]);
            int valueJumpSize = SetValueJumpSize(minValue, maxValue);

            if (valueJumpSize == 0 || (frameDictionary[itemName] + valueJumpSize > maxValue))
                frameDictionary[itemName] = maxValue;
            else
                frameDictionary[itemName] += valueJumpSize;

            return frameDictionary;
        }

        private int SetValueJumpSize(int minValue, int maxValue)
        {
            int valueJumpSize;

            if (this._nativeCheck == NativeCheck.Easy.ToString())
                valueJumpSize = (maxValue - minValue) / 1;
            else if (this._nativeCheck == NativeCheck.Medium.ToString())
                valueJumpSize = (int)Math.Round((double)(maxValue - minValue) / 6);
            else if (this._nativeCheck == NativeCheck.Deep.ToString())
                valueJumpSize = (int)Math.Round((double)(maxValue - minValue) / 9);
            else
                valueJumpSize = 1;

            return valueJumpSize;
        }

        private Dictionary<string, int> FixFrameDictioanryWithCorrelater(int index, Dictionary<string, int> frameDictionary)
        {
            int corrolate = frameDictionary[this._itemsNameArr[index]];
            index++;
            string itemName = this._itemGetParmeters.NameOfItem(this._icdItemsDictionary[this._itemsNameArr[index]]);

            while (!itemName.Contains("correlator") && index < this._itemsNameArr.Length)
            {
                if (corrolate == ConvertingClass.ConvertCorrelateToNumber(this._itemGetParmeters.CorrValueOfItem(this._icdItemsDictionary[itemName])))
                    frameDictionary[itemName] = this._itemGetParmeters.MinValueOfItem(this._icdItemsDictionary[itemName]);
                else
                {
                    if (frameDictionary.ContainsKey(itemName))
                        frameDictionary.Remove(itemName);
                }

                index++;

                if (index >= this._itemsNameArr.Length)
                    break;
                else
                    itemName = this._itemGetParmeters.NameOfItem(this._icdItemsDictionary[this._itemsNameArr[index]]);
            }

            return frameDictionary;
        }
    }
}
