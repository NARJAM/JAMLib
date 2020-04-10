using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSenderController<PSM, IM> : StreamSenderController<InputPack<IM>>
{
    public IMasterController<PSM, IM> masterController;
    public Dictionary<int, TickModel<PSM, IM>> tickHistory = new Dictionary<int, TickModel<PSM, IM>>();
    public int tickHistorySize=500;
    public int tickTrack;

    public StreamSenderConfigModel inputSenderConfig = new StreamSenderConfigModel {
         sendRate = 10,
         historySize = 10,
         gameAuth = GameAuth.Server,
    };

    public InputSenderController(IMasterController<PSM, IM> _masterController): base(_masterController.signalRController, _masterController)
    {
        streamSenderConfig = inputSenderConfig;
        masterController = _masterController;
    }

    public override InputPack<IM> GetData()
    {
        InputPack<IM> pi = new InputPack<IM>();
        pi.inputData = masterController.inputController.SampleInput();
        pi.serverEventRequests = masterController.liveController.SampleServerRequests();
        pi.tick = tickTrack;
        PSM psm = masterController.liveController.ProcessPack(pi);
        masterController.ProcessServerRequests(pi.serverEventRequests);
        masterController.mirrorPlayer.SetFromModel(psm);

        TickModel<PSM,IM> tm = new TickModel<PSM, IM>();
        tm.state = psm;
        tm.input = pi.inputData;
        tm.tick = tickTrack;
        AddToHistory(tm);

        tickTrack++;
        return pi;
    }


    void AddToHistory(TickModel<PSM, IM> tm)
    {
        tickHistory.Add(tm.tick, tm);

        if (tickHistory.Count > tickHistorySize)
        {
            tickHistory.Remove(tm.tick - tickHistorySize);
        }

    }


}

[System.Serializable]
public struct TickModel<PSM,IM>
{
    public int tick;
    public IM input;
    public PSM state;
}