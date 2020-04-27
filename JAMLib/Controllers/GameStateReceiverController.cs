using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public class GameStateReceiverController : StreamReceiverController<ServerMessagePack>
    {
        public GameStateReceiverController() : base(IMultiplayerController.m_instance, new ServerMessagePack())
        {
            streamReceiverConfig = IMultiplayerController.config.gameStateReceiverConfig;

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