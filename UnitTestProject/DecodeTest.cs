using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using DecoderLibrary;
using System;
using System.Threading.Tasks.Dataflow;

namespace UnitTests
{
    [TestClass]
    public class DecodeTest
    {
        [TestMethod]
        public void Decode_RandomFrame_Test()
        {
            FlightBoxItemParameters flightBoxItemParameters = new FlightBoxItemParameters();
            FlightBoxEncoder flightBoxEncoder = new FlightBoxEncoder();
            FlightBoxDecoder flightBoxDecoder = new FlightBoxDecoder();

            Dictionary<string, FlightBoxItem> icdItems = AuxiliaryFunctions.ConvertToIcdItemDictionary<FlightBoxItem, FlightBoxItemParameters>(flightBoxItemParameters);
            CreatingSetsForCheck<FlightBoxItem, FlightBoxItemParameters> creatingSetsForCheck = new CreatingSetsForCheck<FlightBoxItem, FlightBoxItemParameters>(flightBoxItemParameters, icdItems, "");

            Dictionary<string, int> frameDictionary = creatingSetsForCheck.CreateFrameDictionaryManager(true);
            List<byte> byteEncoder = AuxiliaryFunctions.Encode(icdItems, frameDictionary, flightBoxItemParameters, flightBoxEncoder);
            Dictionary<string, int> decodeFrame = AuxiliaryFunctions.Decode(byteEncoder, icdItems, flightBoxItemParameters, flightBoxEncoder, flightBoxDecoder);

            foreach (string name in frameDictionary.Keys)
                Assert.AreEqual(frameDictionary[name], decodeFrame[name]);
        }

        [TestMethod]
        public void Decode_ConstFrame_Test()
        {
            FlightBoxItemParameters flightBoxItemParameters = new FlightBoxItemParameters();
            FlightBoxEncoder flightBoxEncoder = new FlightBoxEncoder();
            FlightBoxDecoder flightBoxDecoder = new FlightBoxDecoder();

            Dictionary<string, FlightBoxItem> icdItems = AuxiliaryFunctions.ConvertToIcdItemDictionary<FlightBoxItem, FlightBoxItemParameters>(flightBoxItemParameters);
            Dictionary<string, int> frameDictionary = AuxiliaryFunctions.CreateFrameDictionary(icdItems);

            List<byte> byteEncoder = AuxiliaryFunctions.Encode(icdItems, frameDictionary, flightBoxItemParameters, flightBoxEncoder);
            Dictionary<string, int> decodeFrame = AuxiliaryFunctions.Decode(byteEncoder, icdItems, flightBoxItemParameters, flightBoxEncoder, flightBoxDecoder);

            foreach (string name in frameDictionary.Keys)
                Assert.AreEqual(frameDictionary[name], decodeFrame[name]);
        }

        //[TestMethod]
        //public void Pipeline_Correct()
        //{
        //    FlightBoxItemParameters flightBoxItemParameters = new FlightBoxItemParameters();
        //    FlightBoxEncoder flightBoxEncoder = new FlightBoxEncoder();
        //    FlightBoxDecoder flightBoxDecoder = new FlightBoxDecoder();

        //    Dictionary<string, FlightBoxItem> icdItems = AuxiliaryFunctions.ConvertToIcdItemDictionary<FlightBoxItem, FlightBoxItemParameters>(flightBoxItemParameters);
        //    Dictionary<string, int> frameDictionary = AuxiliaryFunctions.CreateFrameDictionary(icdItems);
        //    List<byte> rawMessage = AuxiliaryFunctions.Encode(icdItems, frameDictionary, flightBoxItemParameters, flightBoxEncoder);

        //    DecoderManager<FlightBoxItem, FlightBoxItemParameters, FlightBoxEncoder, FlightBoxDecoder> decoderManager = new DecoderManager<FlightBoxItem, FlightBoxItemParameters, FlightBoxEncoder, FlightBoxDecoder>(-1);
        //    Tuple<Dictionary<string, FlightBoxItem>, Dictionary<string, int>, Dictionary<string, int>, List<byte>, FlightBoxItemParameters, FlightBoxEncoder, FlightBoxDecoder> rawMessageTuple =
        //        Tuple.Create(icdItems, frameDictionary, frameDictionary, rawMessage, flightBoxItemParameters, flightBoxEncoder, flightBoxDecoder);

        //    decoderManager.PullerBlock.Post(rawMessageTuple);
        //    decoderManager.PullerBlock.Complete();
        //    decoderManager.De.Completion;

        //    return decoderManager;
        //}
    }
}
