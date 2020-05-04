using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JAMLib
{
    public class InputReceiverController : StreamReceiverController<ClientMessagePack>
    {
        public IMasterController masterController;

        public InputReceiverController(IMasterController _masterController) : base(_masterController, new ClientMessagePack())
        {
            streamReceiverConfig = IMultiplayerController.config.inputReceiverConfig;
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
            masterController.ProcessServerRequests(data.serverEventRequestModel);
        }

    }
}