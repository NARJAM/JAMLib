using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameStateReceiverController<GSM, PSM, IM, PIM> : StreamReceiverController<GameStatePack<GSM, PSM, PIM>, GSM, PSM, IM, PIM>
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

    public GameStateReceiverController() : base(IMultiplayerController<GSM, PSM, IM, PIM>.instance)
    {
        streamReceiverConfig = gameStateReceiverConfig;
    }

    public IMultiplayerController<GSM, PSM, IM, PIM> gameController;

    public override DataPackage<GameStatePack<GSM, PSM, PIM>> GetPredictedPackage(DataPackage<GameStatePack<GSM, PSM, PIM>> data)
    {
        DataPackage<GameStatePack<GSM, PSM, PIM>> predictedPackage = data;

        return predictedPackage;
    }

    public override void ProcessData(GameStatePack<GSM, PSM, PIM> data)
    {
        gameController.ProcessGameStatePack(data);
    }

}
