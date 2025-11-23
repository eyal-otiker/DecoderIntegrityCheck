using System;
using DecoderLibrary;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UnitTests
{
    static class AuxiliaryFunctions
    {
        const string NAME_OF_LIBRARY = @"\DecoderLibrary";
        const string FILES_LOCATION = @"\Files";
        const string ICD_FILES = @"\ICDFiles\";

        public static string FindMainFolderOfProject()
        {
            string folderName = Directory.GetCurrentDirectory();
            string parentFolder; int indexOfParentFolder;

            while (!folderName.EndsWith(NAME_OF_LIBRARY))
            {
                parentFolder = Directory.GetParent(folderName).Name;
                indexOfParentFolder = folderName.IndexOf(parentFolder);
                folderName = folderName.Remove(indexOfParentFolder + parentFolder.Length);
            }

            return folderName;
        }

        public static string GetIcdText()
        {
            string icdFile = FindMainFolderOfProject() + NAME_OF_LIBRARY + FILES_LOCATION + ICD_FILES + "FlightBoxUpICD.json";
            return File.ReadAllText(icdFile);
        }

        public static Dictionary<string, DataType> ConvertToIcdItemDictionary<DataType, GetParametersType>(GetParametersType parametersItem) where GetParametersType : IIcdItemParameters<DataType>
        {
            JsonToIcdItemConverter<DataType, GetParametersType> jsonToIcdItemConverter = new JsonToIcdItemConverter<DataType, GetParametersType>(GetIcdText(), parametersItem);
            Dictionary<string, DataType> icdItems = jsonToIcdItemConverter.ConverterManager();
            return icdItems;
        }

        public static Dictionary<string, int> CreateFrameDictionary<DataType>(Dictionary<string, DataType> icdItems)
        {
            Dictionary<string, int> frameDictionary = new Dictionary<string, int>();
            int[] numbersInDictionary = new int[] { 172, 6, 200, 3, 0, 1, 0, 1, 0, 1, 1, 0, 0, 4, 89, 67, 121 };
            int i = 0;

            foreach (string nameOfItem in icdItems.Keys)
            {
                frameDictionary.Add(nameOfItem, numbersInDictionary[i]);
                i++;
            }

            return frameDictionary;
        }

        public static List<byte> Encode<DataType, GetParametersType, EncoderType>(Dictionary<string, DataType> icdItems, Dictionary<string, int> frameDictionary, GetParametersType parametersItem, EncoderType encoderItem) where GetParametersType : IIcdItemParameters<DataType> where EncoderType : IIcdItemEncoder<DataType>
        {
            IcdItemEncoder<DataType, GetParametersType, EncoderType> icdItemEncoder = new IcdItemEncoder<DataType, GetParametersType, EncoderType>(parametersItem, encoderItem);
            List<byte> byteEncoder = icdItemEncoder.EncodeToRawMessage(icdItems, frameDictionary);

            return byteEncoder;
        }

        public static Dictionary<string, int> Decode<DataType, GetParametersType, EncoderType, DecoderType>(List<byte> listEncoded, Dictionary<string, DataType> icdItems, GetParametersType parametersItem, EncoderType encoderItem, DecoderType decoderItem) where GetParametersType : IIcdItemParameters<DataType> where EncoderType : IIcdItemEncoder<DataType> where DecoderType : IRawMessageDecoder<DataType, EncoderType>
        {
            RawMessageDecoder<DataType, GetParametersType, EncoderType, DecoderType> rawMessageDecoder =
                new RawMessageDecoder<DataType, GetParametersType, EncoderType, DecoderType>(parametersItem, encoderItem, decoderItem);
            Dictionary<string, int> decodeFrame = rawMessageDecoder.DecodeToFrame(icdItems, listEncoded);
            return decodeFrame;
        }
    }
}
