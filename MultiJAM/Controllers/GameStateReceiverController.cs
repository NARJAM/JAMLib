using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameStateReceiverController : StreamReceiverController<GameStatePack>
{
    public StreamReceiverConfigModel gameStateReceiverConfig = new StreamReceiverConfigModel {
        isFlexibleProcessing = true,
        bufferCountMaxout = 4,
        bufferCountMin = 0,
        bufferCountIdeal = 2,
        initialBufferCount = 2,
        gameAuth = GameAuth.Client,
        processMode = ProcessMode.Ideal
};
    
    public GameStateReceiverController() : base(IMultiplayerController.iinstance)
    {
        streamReceiverConfig = gameStateReceiverConfig;
     
    }

    public override void ProcessData(GameStatePack data)
    {
        IMultiplayerController.iinstance.ProcessGameStatePack(data);
    }

    public override void InitStreamReception(string eventName)
    {
        IMultiplayerController.iinstance.transportController.IOnFromServer(eventName, DataPackageReceived);
        StartReception();
    }
}
