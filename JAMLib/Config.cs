
namespace JAMLib
{
    public class Config
    {
        public string hubName = "flow";
        public string serverUrl = "https://flowmanager20200604071117.azurewebsites.net/";
        public bool isClientSidePrediction;
        public float maxCorrectionError = 1f;

        public StreamSenderConfigModel inputSenderConfig = new StreamSenderConfigModel
        {
            sendRate = 5,
            historySize = 10,
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
            historySize = 2,
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