﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSenderController<PSM, IM, PIM> : StreamSenderController<InputPack<IM>, PIM>
{
    public IMasterController<PSM, IM, PIM> masterController;
    public Dictionary<int, TickModel<PSM, IM>> tickHistory = new Dictionary<int, TickModel<PSM, IM>>();
    public int tickHistorySize=50000;
    public int tickTrack;

    public StreamSenderConfigModel inputSenderConfig = new StreamSenderConfigModel {
         sendRate = 5,
         historySize = 10,
         gameAuth = GameAuth.Server,
    };

    public InputSenderController(IMasterController<PSM, IM,PIM> _masterController): base(_masterController.signalRController, _masterController)
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
        masterController.SetMirrorState(psm);

        TickModel<PSM,IM> tm = new TickModel<PSM, IM>();
        tm.state = psm;
        tm.input = pi.inputData;
        tm.tick = tickTrack;
        AddToHistory(tm);
        return pi;
    }


    void AddToHistory(TickModel<PSM, IM> tm)
    {
        tickHistory.Add(tm.tick, tm);

        if (tickHistory.Count > tickHistorySize)
        {
            tickHistory.Remove(tm.tick - tickHistorySize);
        }

        tickTrack++;
    }


}

[System.Serializable]
public struct TickModel<PSM,IM>
{
    public int tick;
    public IM input;
    public PSM state;
}