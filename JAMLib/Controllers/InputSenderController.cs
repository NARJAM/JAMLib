using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JAMLib
{
    public class InputSenderController : StreamSenderController<ClientMessagePack>
    {
        public Dictionary<int, TickModel> tickHistory = new Dictionary<int, TickModel>();
        public int tickHistorySize = 50000;
        public int tickTrack;

        public IMasterController masterController;
        public InputSenderController(IMasterController _masterController) : base(_masterController)
        {
            streamSenderConfig = IMultiplayerController.config.inputSenderConfig;
            masterController = _masterController;
        }

        public override ClientMessagePack GetData()
        {
            ClientMessagePack pi = new ClientMessagePack();
            pi.inputData = masterController.inputController.SampleInput();
            pi.serverEventRequestModel = masterController.liveController.SampleServerRequests();
            pi.tick = tickTrack;
            PlayerStateModel psm = masterController.liveController.ProcessPack(pi);
            masterController.ProcessServerRequests(pi.serverEventRequestModel);
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
}