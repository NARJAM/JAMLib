public class StreamReceiverConfigModel
{
    public bool isFlexibleProcessing;
    public int bufferCountMaxout;
    public int bufferCountMin;
    public int bufferCountIdeal;
    public int initialBufferCount = 3;
    public GameAuth gameAuth;
    public ProcessMode processMode = ProcessMode.Ideal;
}
