using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace DecoderLibrary
{
    public class TelemetryDataProcessing<IcdDataType, GetParametersType, EncoderType, DecoderType> where GetParametersType : IIcdItemParameters<IcdDataType> where EncoderType : IIcdItemEncoder<IcdDataType> where DecoderType : IRawMessageDecoder<IcdDataType, EncoderType>
    {
        public TransformBlock<PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>, PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>> PullerBlock; // icdItemsDictionary, frameDictionary, clientDictionary, rawMessage, convert, encoder, decoder
        private TransformBlock<PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>, CompriasionBlockItem<IcdDataType, GetParametersType>> _decoderBlock; // RETURN: serverDictionary, clientDictionary, convert, frameDictionary, icdItemsDictionary
        public ActionBlock<CompriasionBlockItem<IcdDataType, GetParametersType>> CompirasionBlock;
        public BufferBlock<string> AlertCollectionBlock;
        private int _checkCount;
        public List<string> ProperList;
        public List<string> NotProperList;
        public List<string> NotValidList;

        public TelemetryDataProcessing(int checkCount)
        {
            CreatePullerBlock();
            CreateDecoderBlock();
            CreateCompirasionBlock();
            CreateAlertCollectionBlock();
            CreatePipeLine();
            this._checkCount = checkCount;
            this.ProperList = new List<string>();
            this.NotProperList = new List<string>();
            this.NotValidList = new List<string>();
        }

        private void CreatePullerBlock()
        {
            this.PullerBlock = new TransformBlock<PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>,
                PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>>(tupleItems =>
            {
                return tupleItems;
            });
        }

        private void CreateDecoderBlock()
        {
            this._decoderBlock = new TransformBlock<PullerBlockItem<IcdDataType, GetParametersType, EncoderType, DecoderType>, CompriasionBlockItem<IcdDataType, GetParametersType>>(pullerBlockItem =>
            {
                RawMessageDecoder<IcdDataType, GetParametersType, EncoderType, DecoderType> decodeFrameObject = new RawMessageDecoder<IcdDataType, GetParametersType, EncoderType, DecoderType>(pullerBlockItem.GetParametersItem, pullerBlockItem.Encoder, pullerBlockItem.Decoder);
                Dictionary<string, int> decodeFrameServerDictionary = decodeFrameObject.DecodeToFrame(pullerBlockItem.IcdItemsDictionary, pullerBlockItem.RawMessage);
                CompriasionBlockItem<IcdDataType, GetParametersType> decodersDictionary = new CompriasionBlockItem<IcdDataType, GetParametersType>(decodeFrameServerDictionary, pullerBlockItem.ClientDictionary, pullerBlockItem.GetParametersItem, pullerBlockItem.FrameDictionary, pullerBlockItem.IcdItemsDictionary);

                return decodersDictionary;
            });
        }

        private void CreateCompirasionBlock()
        {
            this.CompirasionBlock = new ActionBlock<CompriasionBlockItem<IcdDataType, GetParametersType>>(tupleItem =>
            {
                int countProper = 0; int countNotProper = 0; int countNotValid = 0; int count = 0;

                this.AlertCollectionBlock.Post("Check " + (this._checkCount + 1).ToString());
                _checkCount++;

                foreach (string nameOfItem in tupleItem.ClientDictionary.Keys)
                {
                    if (tupleItem.GetParametersItem.LocationOfItem(tupleItem.IcdItemsDictionary[nameOfItem]) != -1)
                    {
                        if (tupleItem.DecodeServerFrameDictionary.ContainsKey(nameOfItem) &&
                        tupleItem.DecodeServerFrameDictionary[nameOfItem] == tupleItem.ClientDictionary[nameOfItem])
                        {
                            countProper++;
                            this.AlertCollectionBlock.Post(nameOfItem + " proper" + "(value send to your decoder: " + tupleItem.FrameDictionary[nameOfItem] +
                                ", value after your decoder: " + tupleItem.ClientDictionary[nameOfItem] + ")");
                            this.ProperList.Add(nameOfItem);
                        }
                        else
                        {
                            if (tupleItem.GetParametersItem.MinValueOfItem(tupleItem.IcdItemsDictionary[nameOfItem]) > tupleItem.ClientDictionary[nameOfItem] ||
                                    tupleItem.GetParametersItem.MaxValueOfItem(tupleItem.IcdItemsDictionary[nameOfItem]) < tupleItem.ClientDictionary[nameOfItem])
                            {
                                countNotValid++;
                                this.AlertCollectionBlock.Post(nameOfItem + " not valid" + "(value send to your decoder: " + tupleItem.FrameDictionary[nameOfItem] +
                                ", value after your decoder: " + tupleItem.ClientDictionary[nameOfItem] + ") " + "(possible range of values according to ICD file: " + 
                                tupleItem.GetParametersItem.MinValueOfItem(tupleItem.IcdItemsDictionary[nameOfItem]) + "-" + tupleItem.GetParametersItem.MaxValueOfItem(tupleItem.IcdItemsDictionary[nameOfItem]) + ")");
                                this.NotValidList.Add(nameOfItem);
                            }
                            else
                            {
                                countNotProper++;
                                this.AlertCollectionBlock.Post(nameOfItem + " not proper" + "(value send to your decoder: " + tupleItem.FrameDictionary[nameOfItem] +
                                ", value after your decoder: " + tupleItem.ClientDictionary[nameOfItem] + ")");
                                this.NotProperList.Add(nameOfItem);
                            }
                        }
                        count++;
                    }
                }

                this.AlertCollectionBlock.Post(countProper.ToString());
                this.AlertCollectionBlock.Post(countNotProper.ToString());
                this.AlertCollectionBlock.Post(countNotValid.ToString());

                this.AlertCollectionBlock.Post(Math.Round((double)countProper / count * 100, 1).ToString());
                this.AlertCollectionBlock.Post(Math.Round((double)countNotProper / count * 100, 1).ToString());
                this.AlertCollectionBlock.Post(Math.Round((double)countNotValid / count * 100, 1).ToString());
            });
        }

        public void CreateAlertCollectionBlock()
        {
            this.AlertCollectionBlock = new BufferBlock<string>();
        }

        public void CreatePipeLine()
        {
            DataflowLinkOptions linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            this.PullerBlock.LinkTo(this._decoderBlock, linkOptions);
            this._decoderBlock.LinkTo(this.CompirasionBlock, linkOptions);
        }
    }
}
