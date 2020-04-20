using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JAMLib
{
    public class InputReceiverController : StreamReceiverController<ClientMessagePack>
    {
        public IMasterController masterController;

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

        public InputReceiverController(IMasterController _masterController) : base(_masterController)
        {
            streamReceiverConfig = inputReceiverConfig;
            masterController = _masterController;
        }

        public override void InitStreamReception(string eventName)
        {
            IMultiplayerController.m_instance.transportController.IOnFromClient(eventName, DataPackageReceived);
            StartReception();
        }

        public override void ProcessData(ClientMessagePack data)
        {
            masterController.SetMirrorState(masterController.liveController.ProcessPack(data));
            masterController.ProcessServerRequests(data.serverEventRequests);

        }

    }
}