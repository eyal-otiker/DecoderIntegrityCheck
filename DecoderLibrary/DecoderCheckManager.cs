using Newtonsoft.Json;
//using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace DecoderLibrary
{
    public class DecoderCheckManager
    {
        private Stopwatch _checkTime;
        private int _checkCount;
        private readonly bool _isRandomCheck;
        private readonly string _nativeCheck;
        private readonly string _icdText;
        private readonly string _clientDecoderFileLocation;
        private readonly string _settingFileText;
        private readonly string _ngpFileLocation;
        private Dictionary<string, int> _frameDictionary;
        private readonly double[] _summaryCheckArr;
        private ClientDecoderDetail _clientDecoderDetail;
        private double _frameSetsCheckNumber;
        private string _userName;
        private List<string> _properList;
        private List<string> _notProperList;
        private List<string> _notValidList;
        //private Logger _logger;

        public DecoderCheckManager(string icdText, string dllDecoderClientLocation, string nativeCheck, bool isRandomCheck, string settingFileText, string ngpFileLocation, string userName/*, Logger logger*/)
        {
            this._icdText = icdText;
            this._clientDecoderFileLocation = dllDecoderClientLocation;
            this._nativeCheck = nativeCheck;
            this._isRandomCheck = isRandomCheck;
            this._settingFileText = settingFileText;
            this._ngpFileLocation = ngpFileLocation;
            this._checkCount = 0;
            this._summaryCheckArr = new double[3];
            this._frameSetsCheckNumber = 0;
            this._userName = userName;
            this._properList = new List<string>();
            this._notProperList = new List<string>();
            this._notValidList = new List<string>();
            //this._logger = logger;
        }
        
        public List<string> NavigateByIcdType()
        {
            if (this._icdText.Contains("CorrValue"))
            {
                FiberBoxItemParameters fiberBoxItemParameters = new FiberBoxItemParameters();
                FiberBoxEncoder fiberBoxEncoder = new FiberBoxEncoder();
                FiberBoxDecoder fiberBoxDecoder = new FiberBoxDecoder();
                return ManageDecoderCheck<FiberBoxItem, FiberBoxItemParameters, FiberBoxEncoder, FiberBoxDecoder>(fiberBoxItemParameters, fiberBoxEncoder, fiberBoxDecoder);
            }
            else
            {
                FlightBoxItemParameters flightBoxItemParameters = new FlightBoxItemParameters();
                FlightBoxEncoder flightBoxEncoder = new FlightBoxEncoder();
                FlightBoxDecoder flightBoxDecoder = new FlightBoxDecoder();
                return ManageDecoderCheck<FlightBoxItem, FlightBoxItemParameters, FlightBoxEncoder, FlightBoxDecoder>(flightBoxItemParameters, flightBoxEncoder, flightBoxDecoder);
            }
        }

        private List<string> ManageDecoderCheck<IcdDataType, GetParametersType, EncoderType, DecoderType>(GetParametersType getParametersItem, EncoderType encoder, DecoderType decoder) 
            where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
        {
            List<string> alertCollection = new List<string>();

            try
            {
                JsonToIcdItemConverter<IcdDataType, GetParametersType> jsonToIcdItemConvertor = new JsonToIcdItemConverter<IcdDataType, GetParametersType>(this._icdText, getParametersItem);
                Dictionary<string, IcdDataType> dictionaryItems = jsonToIcdItemConvertor.ConverterManager();
                alertCollection = SetParameters(dictionaryItems, getParametersItem);

                if (this._frameDictionary != null && this._checkCount < this._frameSetsCheckNumber)
                {
                    Dictionary<string, int>  clientDictionary = CallToClientDecoder(dictionaryItems);

                    if (clientDictionary != null)
                    {
                        List<byte> rawMessage = EncodeToRawMessage(dictionaryItems, getParametersItem, encoder, this._frameDictionary);
                        TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType> telemetryDataProcessing = ConnectToDecoderManager(dictionaryItems, clientDictionary, rawMessage, getParametersItem, encoder, decoder);
                        alertCollection.AddRange(BuildListAlertCollection(telemetryDataProcessing));
                    }
                    else
                        alertCollection.Add("The ICD file is not appropriate to your decoder");

                    this._checkCount++;
                }
            }
            catch (Exception ex) when (ex is JsonSerializationException)                                       
            {
                //this._logger.Error(ex, "You upload incorrect ICD file");
                alertCollection.Add("You upload incorrect ICD file");
            }
                
            return alertCollection;
        }

        private List<string> SetParameters<IcdDataType, GetParametersType>(Dictionary<string, IcdDataType> dictionaryItems, GetParametersType getParametersItem) where GetParametersType : IIcdItemParameters<IcdDataType>
        {
            List<string> alertCollection = new List<string>();
            CreatingSetsForCheck<IcdDataType, GetParametersType> creatingSetObject = new CreatingSetsForCheck<IcdDataType, GetParametersType>(getParametersItem, dictionaryItems, this._nativeCheck);

            if (this._checkCount == 0)
            {
                if (this._isRandomCheck == true)
                    this._frameSetsCheckNumber = Math.Round(FrameOptionsNumberCalculation.CalculateOptionsSetsNumber(dictionaryItems, getParametersItem) * FrameOptionsNumberCalculation.GetPrecentOptionsInRandomCheck(this._nativeCheck), 0);
                else
                    this._frameSetsCheckNumber = Math.Round(FrameOptionsNumberCalculation.CalculateOptionsNumberInSerialCheck(dictionaryItems, getParametersItem, this._nativeCheck), 0);

                if (this._nativeCheck != string.Empty) // not calculate check time
                {
                    try
                    {
                        this._clientDecoderDetail = JsonConvert.DeserializeObject<ClientDecoderDetail>(this._settingFileText);
                        CheckSettingFileContent();
                        double optionsOfSetsNum = FrameOptionsNumberCalculation.CalculateOptionsSetsNumber(dictionaryItems, getParametersItem);
                        alertCollection = new List<string> { optionsOfSetsNum.ToString(), this._frameSetsCheckNumber.ToString(),
                            this._nativeCheck, this._isRandomCheck.ToString(), this._clientDecoderDetail.DecoderName, 
                            this._clientDecoderDetail.DataLink.ToString(), this._clientDecoderDetail.Version, this._clientDecoderDetail.DecoderWritingDate.ToString()};
                        //this._frameSetsCheckNumber = 20; // TODO: remove this line
                    }
                    catch (Exception ex) when (ex is JsonSerializationException)
                    {
                        alertCollection = new List<string>(){ "You upload incorrect setting file" };
                        //this._logger.Error(ex, "You uploaded incorrect setting file");
                    }
                }
                this._frameDictionary = creatingSetObject.CreateFrameDictionaryManager(this._isRandomCheck);
            }
            else
                this._frameDictionary = creatingSetObject.CreateFrameDictionaryManager(this._isRandomCheck, this._frameDictionary);

            return alertCollection;
        }

        private void CheckSettingFileContent()
        {
            if (this._clientDecoderDetail.DecoderName == null)
                this._clientDecoderDetail.DecoderName = "null";
            if (this._clientDecoderDetail.Version == null)
                this._clientDecoderDetail.Version = "null";
            if (this._clientDecoderDetail.DecoderWritingDate == null)
                this._clientDecoderDetail.DecoderWritingDate = "null";
        }

        private Dictionary<string, int> CallToClientDecoder<IcdDataType>(Dictionary<string, IcdDataType> dictionaryItems)
        {
            RunningClientDecoder runningDecoderDLL = new RunningClientDecoder();
            return runningDecoderDLL.CallToClientDecoder(dictionaryItems, this._frameDictionary, this._clientDecoderFileLocation);
        }

        private List<byte> EncodeToRawMessage<IcdDataType, GetParametersType, EncoderType>(Dictionary<string, IcdDataType> icdItemsDictionary, GetParametersType getParametersItem, EncoderType encodeObject, Dictionary<string, int> frameDicrionary = null) where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType>
        {
            this._checkTime = new Stopwatch();
            this._checkTime.Start();

            IcdItemEncoder<IcdDataType, GetParametersType, EncoderType> ecnodeSimulatorObject = new IcdItemEncoder<IcdDataType, GetParametersType, EncoderType>(getParametersItem, encodeObject);
            return ecnodeSimulatorObject.EncodeToRawMessage(icdItemsDictionary, frameDicrionary);
        }

        private TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType> ConnectToDecoderManager<IcdDataType, GetParametersType, EncoderType, DecoderType>(Dictionary<string, IcdDataType> icdItemsDictionary, Dictionary<string, int> clientDictionary, List<byte> rawMessage, GetParametersType convertItem, EncoderType encoder, DecoderType decoder) where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
        {
            TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType> telemetryDataProcessing = new TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType>(this._checkCount);
            PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType> pullerBlockItem = new PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>
                (icdItemsDictionary, this._frameDictionary, clientDictionary, rawMessage, convertItem, encoder, decoder);         

            telemetryDataProcessing.PullerBlock.Post(pullerBlockItem);
            telemetryDataProcessing.PullerBlock.Complete();
            telemetryDataProcessing.CompirasionBlock.Completion.Wait();

            return telemetryDataProcessing;
        }

        private List<string> BuildListAlertCollection<IcdDataType, GetParametersType, EncoderType, DecoderType>(TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType> decoderManager) where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
        {
            List<string> alertCollectionList = new List<string> { StopTimer() };

            if (this._nativeCheck != string.Empty)
            {              
                while (decoderManager.AlertCollectionBlock.TryReceive(out string value))
                    alertCollectionList.Add(value);

                SetParmatersResultLists(decoderManager);


                for (int i = 2; i >= 0; i--)
                    this._summaryCheckArr[i] += double.Parse(alertCollectionList.ToArray()[alertCollectionList.Count - 1 - (2 - i)]);

                if (this._checkCount == this._frameSetsCheckNumber - 1) // checkCount start from 0
                {
                    List<string> summaryAlertCollectionList = BuildListSummaryAlertCollection();
                    ConnectToMongoDb(summaryAlertCollectionList, typeof(IcdDataType).Name, this._clientDecoderDetail);
                }
            }        

            return alertCollectionList;
        }

        private void SetParmatersResultLists<IcdDataType, GetParametersType, EncoderType, DecoderType>(TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType> decoderManager) where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
        {
            if (this._checkCount == 0) // first check
            {
                this._notProperList.AddRange(decoderManager.NotProperList);
                this._notValidList.AddRange(decoderManager.NotValidList);
            }
            else
            {
                foreach (string nameOfItem in decoderManager.NotValidList)
                {
                    if (!this._notValidList.Contains(nameOfItem))
                    {
                        this._notValidList.Add(nameOfItem);
                        if (this._notProperList.Contains(nameOfItem))
                            this._notProperList.Remove(nameOfItem);
                    }
                }

                foreach (string nameOfItem in decoderManager.NotProperList)
                {
                    if (!this._notValidList.Contains(nameOfItem) && !this._notProperList.Contains(nameOfItem))
                        this._notProperList.Add(nameOfItem);
                }

                if (this._checkCount == this._frameSetsCheckNumber - 1)
                {
                    foreach (string nameOfItem in decoderManager.ProperList)
                    {
                        if (!this._notValidList.Contains(nameOfItem) && !this._notProperList.Contains(nameOfItem))
                            this._properList.Add(nameOfItem);
                    }
                }
            }
        }

        private List<string> BuildListSummaryAlertCollection()
        {
            List<string> summaryAlertCollectionList = new List<string>();
            foreach (double numSummary in this._summaryCheckArr)
                summaryAlertCollectionList.Add(Math.Round(numSummary / (this._checkCount + 1), 2) + "%");
            return summaryAlertCollectionList;
        }

        private void ConnectToMongoDb(List<string> listSummaryAlertCollection, string typeOfIcdItem, ClientDecoderDetail clientDecoderDetail)
        {
            SummaryAlertCollectionSaving summaryAlertCollectionSaving = new SummaryAlertCollectionSaving();
            summaryAlertCollectionSaving.SaveData(listSummaryAlertCollection.ToArray()[0],
                listSummaryAlertCollection.ToArray()[1], listSummaryAlertCollection.ToArray()[2], typeOfIcdItem, this._nativeCheck, this._isRandomCheck, this._ngpFileLocation, clientDecoderDetail, this._userName, this._properList, this._notProperList, this._notValidList);

        }

        private string StopTimer()
        {
            this._checkTime.Stop();
            return this._checkTime.Elapsed.TotalSeconds.ToString();
        }
    }
}
