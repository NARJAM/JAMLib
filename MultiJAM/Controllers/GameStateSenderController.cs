using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateSenderController : StreamSenderController<GameStatePack>
{
    public StreamSenderConfigModel gameStateSenderConfig = new StreamSenderConfigModel {
         sendRate = 5,
         historySize = 2,
         gameAuth = GameAuth.Server,
    };

    public GameStateSenderController() : base(IMultiplayerController.iinstance)
    {
        streamSenderConfig = gameStateSenderConfig;
    }

    public override GameStatePack GetData()
    {
        return GetGameState();
    }
     
    public GameStatePack GetGameState()
    {
        GameStatePack g = new GameStatePack();

        g.playerStates = IMultiplayerController.iinstance.SamplePlayerStates();
        g.gameState = IMultiplayerController.iinstance.SampleGameState();
        return g;
    }

}
