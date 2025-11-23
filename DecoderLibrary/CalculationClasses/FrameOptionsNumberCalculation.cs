using System.Collections.Generic;

namespace DecoderLibrary
{
    public enum NativeCheck
    {
        Easy,
        Medium,
        Deep
    }

    static class FrameOptionsNumberCalculation
    {
        public static double CalculateOptionsSetsNumber<IcdDataType, GetParametersType>(Dictionary<string, IcdDataType> itemsDictionary, GetParametersType itemGetParamaters) where GetParametersType : IIcdItemParameters<IcdDataType>
        {
            double optionsNumber = 1; int optionsNumberItem; int[] optionsInCorrelator = new int[0];

            foreach (string itemName in itemsDictionary.Keys)
            {
                optionsNumberItem = itemGetParamaters.MaxValueOfItem(itemsDictionary[itemName]) - itemGetParamaters.MinValueOfItem(itemsDictionary[itemName]) + 1;

                if (itemGetParamaters.CorrValueOfItem(itemsDictionary[itemName]) == string.Empty)
                {
                    optionsNumber *= optionsNumberItem;
                    if (itemGetParamaters.NameOfItem(itemsDictionary[itemName]).Contains("correlator"))
                    {
                        if (optionsInCorrelator.Length != 0)
                            optionsNumber *= SumOfCorrItemsOptions(optionsInCorrelator);
                        optionsInCorrelator = SetsValueCorrArray(optionsNumberItem);
                    }
                }
                else
                {
                    int corrValue = ConvertingClass.ConvertCorrelateToNumber(itemGetParamaters.CorrValueOfItem(itemsDictionary[itemName]));
                    if (optionsInCorrelator[corrValue] == 0)
                        optionsInCorrelator[corrValue] = optionsNumberItem;
                    else
                        optionsInCorrelator[corrValue] *= optionsNumberItem;
                }
            }

            if (optionsInCorrelator.Length != 0)
                optionsNumber *= SumOfCorrItemsOptions(optionsInCorrelator);

            return optionsNumber;
        }

        public static double GetPrecentOptionsInRandomCheck(string nativeCheck)
        {
            if (nativeCheck == NativeCheck.Easy.ToString())
                return 0.001;
            else if (nativeCheck == NativeCheck.Medium.ToString())
                return 0.005;
            else if (nativeCheck == NativeCheck.Deep.ToString())
                return 0.01;
            else
                return 0.5;
        }

        public static double CalculateOptionsNumberInSerialCheck<IcdDataType, GetParametersType>(Dictionary<string, IcdDataType> icdItemsDictionary, GetParametersType itemParameters, string nativeCheck) where GetParametersType : IIcdItemParameters<IcdDataType>
        {
            double optionsNum = 1;
            int optionsNumItem = OptionsNumberCheckInItem(nativeCheck);

            foreach (string name in icdItemsDictionary.Keys)
            {
                if (itemParameters.MaxValueOfItem(icdItemsDictionary[name]) - itemParameters.MinValueOfItem(icdItemsDictionary[name]) + 1 > optionsNumItem)
                    optionsNum *= optionsNumItem;
                else
                    optionsNum *= (itemParameters.MaxValueOfItem(icdItemsDictionary[name]) - itemParameters.MinValueOfItem(icdItemsDictionary[name]) + 1);
            }

            return optionsNum;
        }

        private static int OptionsNumberCheckInItem(string nativeCheck)
        {
            if (nativeCheck == NativeCheck.Easy.ToString())
                return 3;
            else if (nativeCheck == NativeCheck.Medium.ToString())
                return 7;
            else // deep
                return 10;
        }

        private static int[] SetsValueCorrArray(int size)
        {
            int[] arrCorrItem = new int[size];
            for (int i = 0; i < size; i++)
                arrCorrItem[i] = 0;

            return arrCorrItem;
        }

        private static int SumOfCorrItemsOptions(int[] optionsInCorr)
        {
            int sum = 0;
            for (int i = 0; i < optionsInCorr.Length; i++)
                sum += optionsInCorr[i];
            return sum;
        }
    }
}
