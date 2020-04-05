using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateSenderController<GSM, PSM, IM> : StreamSenderController<GameStatePack<GSM, PSM>>
{
    public StreamSenderConfigModel gameStateSenderConfig = new StreamSenderConfigModel {
         sendRate = 10,
         historySize = 10,
         gameAuth = GameAuth.Server,
    };

    public GameStateSenderController(IMultiplayerController<GSM, PSM, IM> _gameController) : base(_gameController.signalRController, _gameController)
    {
        streamSenderConfig = gameStateSenderConfig;
        gameController = _gameController;
    }

    public IMultiplayerController<GSM, PSM, IM> gameController;

    public override GameStatePack<GSM, PSM> GetData()
    {
        return GetGameState();
    }
     
    public GameStatePack<GSM, PSM> GetGameState()
    {
        GameStatePack <GSM,PSM> g = new GameStatePack<GSM, PSM>();

        g.playerStates = gameController.SamplePlayerStates();
        g.gameStateModel = gameController.SampleGameState();
        return g;
    }

}
