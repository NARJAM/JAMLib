using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSenderController : StreamSenderController<InputPack>
{
    public Dictionary<int, TickModel> tickHistory = new Dictionary<int, TickModel>();
    public int tickHistorySize=50000;
    public int tickTrack;

    public StreamSenderConfigModel inputSenderConfig = new StreamSenderConfigModel {
         sendRate = 5,
         historySize = 10,
         gameAuth = GameAuth.Server,
    };

    public IMasterController masterController;
    public InputSenderController(IMasterController _masterController): base(_masterController)
    {
        streamSenderConfig = inputSenderConfig;
        masterController = _masterController;
    }

    public override InputPack GetData()
    {
        InputPack pi = new InputPack();
       pi.inputData = masterController.inputController.SampleInput();
       pi.serverEventRequests = masterController.liveController.SampleServerRequests();
        pi.tick = tickTrack;
        PlayerStateModel psm = masterController.liveController.ProcessPack(pi);
        masterController.ProcessServerRequests(pi.serverEventRequests);
        masterController.SetMirrorState(psm);

        TickModel tm = new TickModel();
        tm.state = psm;
        tm.input = pi.inputData;
        tm.tick = tickTrack;
        AddToHistory(tm);
        return pi;
    }


    void AddToHistory(TickModel tm)
    {
        tickHistory.Add(tm.tick, tm);

        if (tickHistory.Count > tickHistorySize)
        {
            tickHistory.Remove(tm.tick - tickHistorySize);
        }

        tickTrack++;
    }


}
