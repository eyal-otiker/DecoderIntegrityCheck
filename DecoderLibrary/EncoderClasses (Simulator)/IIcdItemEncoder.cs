using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public interface IIcdItemEncoder<DataType>
    {
        List<string> ExceptionIcdItemList { get; set; }

        bool CheckIfValueInRange(DataType item, int value);

        List<byte> EncodeWithRandomNumber(DataType item, Random rndMinMax, int correlatorValue = -1);

        List<byte> EncodeWithFrameDictioanry(DataType item, int objectValue, int correlatorValue = -1);
    }
}
