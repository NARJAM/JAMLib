
namespace JAMLib
{
    public class Config
    {
        public string hubName = "flow";
        public string serverUrl = "http://localhost:59474/";
        public bool isClientSidePrediction = false;
        public bool isOffline = true;
        public float maxCorrectionError = 1f;
        public int botCount=10;  

        

        public StreamSenderConfigModel inputSenderConfig = new StreamSenderConfigModel
        {
            sendRate = 2,
            historySize = 2,
            gameAuth = GameAuth.Server,
        };

        public StreamReceiverConfigModel inputReceiverConfig = new StreamReceiverConfigModel
        {
            isFlexibleProcessing = true,
            bufferCountMaxout = 4,
            bufferCountMin = 0,
            bufferCountIdeal = 2,
            initialBufferCount = 2,
            gameAuth = GameAuth.Server,
            processMode = ProcessMode.Ideal
        };

        public StreamSenderConfigModel gameStateSenderConfig = new StreamSenderConfigModel
        {
            sendRate = 5,
            historySize = 5,
            gameAuth = GameAuth.Server,
        };

        public StreamReceiverConfigModel gameStateReceiverConfig = new StreamReceiverConfigModel
        {
            isFlexibleProcessing = true,
            bufferCountMaxout = 4,
            bufferCountMin = 0,
            bufferCountIdeal = 2,
            initialBufferCount = 2,
            gameAuth = GameAuth.Client,
            processMode = ProcessMode.Ideal
        };

    }

}