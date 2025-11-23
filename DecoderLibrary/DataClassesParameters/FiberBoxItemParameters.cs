using System;
using System.Collections.Generic;
using System.Text;

namespace DecoderLibrary
{
    public class FiberBoxItemParameters : IIcdItemParameters<FiberBoxItem>
    {
        public string NameOfItem(FiberBoxItem fiberBoxItem)
        {
            return fiberBoxItem.Identifier;
        }

        public int LocationOfItem(FiberBoxItem fiberBoxItem)
        {
            if (fiberBoxItem.Loc != string.Empty && fiberBoxItem.Error == string.Empty)
                return int.Parse(fiberBoxItem.Loc);
            else
                return -1;
        }

        public int MinValueOfItem(FiberBoxItem fiberBoxItem)
        {
            if (fiberBoxItem.PhysicalLimitMin != string.Empty)
            {
                if (int.Parse(fiberBoxItem.PhysicalLimitMax) <= Math.Pow(2, fiberBoxItem.Size))
                    return int.Parse(fiberBoxItem.PhysicalLimitMin);
            }
            return int.Parse(fiberBoxItem.InterfaceLimitMin);
        }

        public int MaxValueOfItem(FiberBoxItem fiberBoxItem)
        {
            if (fiberBoxItem.PhysicalLimitMax != string.Empty)
            {
                if (int.Parse(fiberBoxItem.PhysicalLimitMax) <= Math.Pow(2, fiberBoxItem.Size)) 
                    return int.Parse(fiberBoxItem.PhysicalLimitMax);
            }               
            return int.Parse(fiberBoxItem.InterfaceLimitMax);
        }

        public string MaskOfItem(FiberBoxItem fiberBoxItem)
        {
            return fiberBoxItem.Mask;
        }

        public string CorrValueOfItem(FiberBoxItem fiberBoxItem)
        {
            return fiberBoxItem.CorrValue;
        }

        public int LengthOfItem(FiberBoxItem fiberBoxItem)
        {
            return fiberBoxItem.Size;
        }
    }
}
