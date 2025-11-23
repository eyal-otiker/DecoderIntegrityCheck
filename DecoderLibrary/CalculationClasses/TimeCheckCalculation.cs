using System.Collections.Generic;

namespace DecoderLibrary
{
    public class TimeCheckCalculation
    {
        private readonly string _icdText;
        private readonly bool _isRandomCheck;
        private double _checkTime;
        private readonly string[] _nativesCheck;
        private double _optionsNum;

        public TimeCheckCalculation(string icdText, bool isRandomCheck, double checkTime)
        {
            this._icdText = icdText;
            this._isRandomCheck = isRandomCheck;
            this._checkTime = checkTime;
            this._nativesCheck = new string[] {"Easy", "Medium", "Deep", "Extra Deep"};
        }

        public List<string> CalculationCheckTimeManager()
        {
            if (this._icdText.Contains("CorrValue"))
            {
                FiberBoxItemParameters fiberBoxItemParameters = new FiberBoxItemParameters();
                return ConverterToIcdItem<FiberBoxItem, FiberBoxItemParameters>(fiberBoxItemParameters);
            }
            else
            {
                FlightBoxItemParameters flightBoxItemParameters = new FlightBoxItemParameters();
                return ConverterToIcdItem<FlightBoxItem, FlightBoxItemParameters>(flightBoxItemParameters);
            }
        }

        private List<string> ConverterToIcdItem<IcdDataType, GetParametersType>(GetParametersType itemParameters) where GetParametersType : IIcdItemParameters<IcdDataType>
        {
            JsonToIcdItemConverter<IcdDataType, GetParametersType> jsonToIcdItemConverter = new JsonToIcdItemConverter<IcdDataType, GetParametersType>(this._icdText, itemParameters);
            Dictionary<string, IcdDataType> icdItemsDictionary = jsonToIcdItemConverter.ConverterManager();
            List<string> checkTimeList = new List<string>();

            this._optionsNum = FrameOptionsNumberCalculation.CalculateOptionsSetsNumber(icdItemsDictionary, itemParameters); 
            foreach (string nativeCheck in this._nativesCheck)
            {
                checkTimeList.Add(nativeCheck);
                checkTimeList.Add(CalaculateCheckTime(icdItemsDictionary, itemParameters, nativeCheck).ToString());
            }

            return checkTimeList;
        }

        private double CalaculateCheckTime<IcdDataType, GetParametersType>(Dictionary<string, IcdDataType> icdItemsDictionary, GetParametersType itemParameters, string nativeCheck) where GetParametersType : IIcdItemParameters<IcdDataType>
        {
            double sumCheckTime;

            if (nativeCheck != "Extra Deep")
            {
                if (this._isRandomCheck == true)
                    sumCheckTime = this._optionsNum * FrameOptionsNumberCalculation.GetPrecentOptionsInRandomCheck(nativeCheck) * this._checkTime;
                else
                    sumCheckTime = FrameOptionsNumberCalculation.CalculateOptionsNumberInSerialCheck(icdItemsDictionary, itemParameters, nativeCheck) * this._checkTime;
            }
            else
            {
                if (this._isRandomCheck == true)
                    sumCheckTime = 0.5 * this._optionsNum * this._checkTime;
                else
                    sumCheckTime = this._optionsNum * this._checkTime;
            }

            return sumCheckTime;
        }
    }
}