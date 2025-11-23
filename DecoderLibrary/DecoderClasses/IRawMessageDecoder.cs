using System.Collections.Generic;

namespace DecoderLibrary
{
    public interface IRawMessageDecoder<IcdDataType, EncoderType>
    {
        string DecodeToFrame(IcdDataType item, List<byte> value, EncoderType encoder, int correlator);

        List<byte> GetRawMessageInLocation(IcdDataType item, List<byte> rawMessage);

        string ChangeValueWithMask(IcdDataType item, int rawValue);
    }
}
