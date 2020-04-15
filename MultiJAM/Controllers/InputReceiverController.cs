using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReceiverController<GSM, PSM, IM, PIM> : StreamReceiverController<InputPack<IM>, GSM, PSM, IM, PIM>
{
    public IMasterController<GSM, PSM, IM, PIM> masterController;

    public StreamReceiverConfigModel inputReceiverConfig = new StreamReceiverConfigModel
    {
        isFlexibleProcessing = true,
        bufferCountMaxout = 4,
        bufferCountMin = 0,
        bufferCountIdeal = 2,
        predictionEnabled = true,
        initialBufferCount = 2,
        gameAuth = GameAuth.Server,
        processMode = ProcessMode.Ideal
    };

    public InputReceiverController(IMasterController<GSM, PSM, IM, PIM> _masterController) : base(_masterController)
    {
        streamReceiverConfig = inputReceiverConfig;
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
        masterController.SetMirrorState(masterController.liveController.ProcessPack(data));
        masterController.ProcessServerRequests(data.serverEventRequests);
       
    }

}
