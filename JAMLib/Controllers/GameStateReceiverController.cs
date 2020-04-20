using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public class GameStateReceiverController : StreamReceiverController<ServerMessagePack>
    {
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

        public GameStateReceiverController() : base(IMultiplayerController.m_instance)
        {
            streamReceiverConfig = gameStateReceiverConfig;

        }

        public override void ProcessData(ServerMessagePack data)
        {
            IMultiplayerController.m_instance.ProcessGameStatePack(data);
        }

        public override void InitStreamReception(string eventName)
        {
            IMultiplayerController.m_instance.transportController.IOnFromServer(eventName, DataPackageReceived);
            StartReception();
        }
    }
}