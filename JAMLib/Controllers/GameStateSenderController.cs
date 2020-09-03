using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public class GameStateSenderController : StreamSenderController<ServerMessagePack>
    {
        public GameStateSenderController() : base(IMultiplayerController.m_instance)
        {
            streamSenderConfig = IMultiplayerController.config.gameStateSenderConfig;
        }

        public override void EmitToTransport(string eventName, DataPackageHistory<ServerMessagePack> dataHistory)
        {
            IMultiplayerController.m_instance.transportController.IEmitToClients(eventName, dataHistory);
        }

        public override ServerMessagePack GetData()
        {
            return GetGameState();
        }

        public ServerMessagePack GetGameState()
        {
            ServerMessagePack g = new ServerMessagePack();

            g.playerStates = new List<PlayerStatePack>(IMultiplayerController.m_instance.SamplePlayerStates());
            g.worldState = IMultiplayerController.m_instance.SampleWorldState();
            return g;
        }

    }
}