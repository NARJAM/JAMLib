using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public class GameStateSenderController : StreamSenderController<ServerMessagePack>
    {
        public StreamSenderConfigModel gameStateSenderConfig = new StreamSenderConfigModel
        {
            sendRate = 5,
            historySize = 2,
            gameAuth = GameAuth.Server,
        };

        public GameStateSenderController() : base(IMultiplayerController.m_instance)
        {
            streamSenderConfig = gameStateSenderConfig;
        }

        public override ServerMessagePack GetData()
        {
            return GetGameState();
        }

        public ServerMessagePack GetGameState()
        {
            ServerMessagePack g = new ServerMessagePack();

            g.playerStates = IMultiplayerController.m_instance.SamplePlayerStates();
            g.worldState = IMultiplayerController.m_instance.SampleWorldState();
            return g;
        }

    }
}