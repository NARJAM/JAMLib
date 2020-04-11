using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateSenderController<GSM, PSM, IM, PIM> : StreamSenderController<GameStatePack<GSM, PSM, PIM>,PIM>
{
    public StreamSenderConfigModel gameStateSenderConfig = new StreamSenderConfigModel {
         sendRate = 5,
         historySize = 10,
         gameAuth = GameAuth.Server,
    };

    public GameStateSenderController(IMultiplayerController<GSM, PSM, IM,PIM> _gameController) : base(_gameController.signalRController, _gameController)
    {
        streamSenderConfig = gameStateSenderConfig;
        gameController = _gameController;
    }

    public IMultiplayerController<GSM, PSM, IM, PIM> gameController;

    public override GameStatePack<GSM, PSM, PIM> GetData()
    {
        return GetGameState();
    }
     
    public GameStatePack<GSM, PSM, PIM> GetGameState()
    {
        GameStatePack <GSM,PSM, PIM> g = new GameStatePack<GSM, PSM, PIM>();

        g.playerStates = gameController.SamplePlayerStates();
        g.gameState = gameController.SampleGameState();
        return g;
    }

}
