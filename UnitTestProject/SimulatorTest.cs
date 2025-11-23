using Microsoft.VisualStudio.TestTools.UnitTesting;
using DecoderLibrary;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class SimulatorTest
    {  
        [TestMethod]
        public void Encode_ConstFrame_Test()
        {
            FlightBoxItemParameters flightBoxItemParameters = new FlightBoxItemParameters();
            FlightBoxEncoder flightBoxEncoder = new FlightBoxEncoder();                    

            List<byte> exceptedList = new List<byte>() { 1, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 0,
            0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1,
            0, 1, 1, 1, 1, 0, 0, 1};
            Dictionary<string, FlightBoxItem> icdItems = AuxiliaryFunctions.ConvertToIcdItemDictionary<FlightBoxItem, FlightBoxItemParameters>(flightBoxItemParameters);
            Dictionary<string, int> frameDictionary = AuxiliaryFunctions.CreateFrameDictionary(icdItems);
            List<byte> byteEncoder = AuxiliaryFunctions.Encode(icdItems, frameDictionary, flightBoxItemParameters, flightBoxEncoder);

            for (int i = 0; i < exceptedList.ToArray().Length; i++)
                Assert.AreEqual(exceptedList.ToArray()[i], byteEncoder.ToArray()[i]);
        }
    }
}
