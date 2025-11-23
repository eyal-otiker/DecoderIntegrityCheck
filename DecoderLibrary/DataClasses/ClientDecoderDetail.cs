namespace DecoderLibrary
{
    public enum DataLink
    {
        NULL,
        DownLink,
        UpLink   
    }

    public class ClientDecoderDetail
    {
        public string DecoderName { get; set; }
        public DataLink DataLink { get; set; }
        public string Version { get; set; }
        public string DecoderWritingDate { get; set; }

    }
}
