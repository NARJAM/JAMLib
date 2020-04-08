using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReceiverController<PSM,IM> : StreamReceiverController<InputPack<IM>>
{
    public IMasterController<PSM, IM> masterController;

    public StreamReceiverConfigModel inputReceiverConfig = new StreamReceiverConfigModel
    {
        isFlexibleProcessing = true,
        bufferCountMaxout = 4,
        bufferCountMin = 0,
        bufferCountIdeal = 2,
        predictionEnabled = false,
        initialBufferCount = 2,
        gameAuth = GameAuth.Server,
        processMode = ProcessMode.Ideal
    };

    public InputReceiverController(IMasterController<PSM,IM> _masterController) : base(_masterController.signalRController, _masterController)
    {
        streamReceiverConfig = inputReceiverConfig;
        masterController = _masterController;
    }

    public override DataPackage<InputPack<IM>> GetPredictedPackage(DataPackage<InputPack<IM>> data)
    {
        for(int i=0; i<data.dataStream.Count; i++)
        {
            data.dataStream[i].data.tick += data.dataStream.Count;
        } 

        return data;
    }

    public override void ProcessData(InputPack<IM> data)
    {
        masterController.mirrorPlayer.SetFromModel(masterController.liveController.ProcessPack(data));
    }
}
