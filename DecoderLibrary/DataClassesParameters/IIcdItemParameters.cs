using System;
using System.Collections.Generic;
using System.Text;

namespace DecoderLibrary
{
    public interface IIcdItemParameters<DataType>
    {
        string NameOfItem(DataType item);

        int LocationOfItem(DataType item);

        int MinValueOfItem(DataType item);

        int MaxValueOfItem(DataType item);

        string MaskOfItem(DataType item);

        string CorrValueOfItem(DataType item);

        int LengthOfItem(DataType item);
    }
}
