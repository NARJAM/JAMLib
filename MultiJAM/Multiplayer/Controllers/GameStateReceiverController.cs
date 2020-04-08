using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameStateReceiverController<GSM, PSM, IM> : StreamReceiverController<GameStatePack<GSM, PSM>>
{
    public StreamReceiverConfigModel gameStateReceiverConfig = new StreamReceiverConfigModel {
        isFlexibleProcessing = true,
        bufferCountMaxout = 4,
        bufferCountMin = 0,
        bufferCountIdeal = 2,
        predictionEnabled = false,
        initialBufferCount = 2,
        gameAuth = GameAuth.Client,
        processMode = ProcessMode.Ideal
};

    public GameStateReceiverController(IMultiplayerController<GSM,PSM,IM> _gameController) : base(_gameController.signalRController, _gameController)
    {
        streamReceiverConfig = gameStateReceiverConfig;
        gameController = _gameController;
    }

    public IMultiplayerController<GSM, PSM, IM> gameController;

    public override DataPackage<GameStatePack<GSM, PSM>> GetPredictedPackage(DataPackage<GameStatePack<GSM, PSM>> data)
    {
        DataPackage<GameStatePack<GSM, PSM>> predictedPackage = data;

        return predictedPackage;
    }

    public override void ProcessData(GameStatePack<GSM, PSM> data)
    {
        gameController.ProcessGameStatePack(data);
    }

}
