using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMasterController<GSM, PSM, IM, PIM> : MonoBehaviour
{
    public IPlayerController<GSM, PSM, IM, PIM> liveController;
    public IInputController<IM> inputController;
    public InputReceiverController<GSM, PSM, IM, PIM> inputReceiverController;
    public InputSenderController<GSM, PSM, IM, PIM> inputSenderController;
    public IPlayerView<PSM, PIM> mirrorPlayer;
    public PSM ghostState;
    public PSM projectedState;
    public PSM pastState;
    public ITransportController<GSM, PSM, IM, PIM>transportController;

    public GameObject liveControllerObj;
    public GameObject inputControllerObj;
    public GameObject mirrorPlayerObj;

    public bool isOwner;
    public string connectionId;

    public abstract bool CheckForCorrection(PSM serverState, PSM localState, int tick);

    public void Initialize(ITransportController<GSM, PSM, IM, PIM> _transportController,PlayerStatePack<PSM,PIM> psp, bool _isOwner)
    {
        transportController = _transportController;
        liveController = liveControllerObj.GetComponent<IPlayerController<GSM, PSM, IM, PIM>>();
        inputController = inputControllerObj.GetComponent<IInputController<IM>>();
        mirrorPlayer = mirrorPlayerObj.GetComponent<IPlayerView<PSM, PIM>>();
        liveController.initPlayer = psp.playerInit;
        isOwner = _isOwner;
        connectionId = psp.conId;
        mirrorPlayer.isOwner = _isOwner;
        liveController.Initialize(psp.playerState, _isOwner, connectionId, this);

        if (isOwner)
        {
            inputSenderController = new InputSenderController<GSM, PSM, IM, PIM>(this);
            inputSenderController.StartStream(connectionId);
            inputController.enabled = true;
        }
        else
        {
            inputReceiverController = new InputReceiverController<GSM, PSM, IM, PIM>(this);
            inputReceiverController.StartReception(connectionId);
            inputController.enabled = false;
        }
    }

    public void SetMirrorState(PSM psp)
    {
        mirrorPlayer.SetFromModel(psp);
        OnMirrorStateSet(psp);
    }

    public void SetPlayerInit(PIM pim) 
    {
        mirrorPlayer.SetInit(pim);
    }

    public void SetGhostState(PlayerStatePack<PSM, PIM> psp)
    {
        ghostState = psp.playerState;
        OnGhostStateSet(psp.playerState);
        TickModel<PSM, IM> pastTick = new TickModel<PSM, IM>();

        if (inputSenderController.tickHistory.TryGetValue(psp.tick, out pastTick))
        {
            pastState = pastTick.state; 
            OnPastStateSet(pastState);
            if (CheckForCorrection(psp.playerState, pastTick.state,pastTick.tick))
            {
                ProjectState(psp);
            }
        }
    }

    public abstract void OnMirrorStateSet(PSM playerState);
    public abstract void OnGhostStateSet(PSM playerState);
    public abstract void OnPastStateSet(PSM playerState);

    public void ProjectState(PlayerStatePack<PSM, PIM> psp)
    {
        liveController.SetState(psp.playerState);
        for (int i = psp.tick; i < inputSenderController.tickTrack; i++)
            {
                TickModel<PSM, IM> newTick = new TickModel<PSM, IM>();
                projectedState = liveController.ProcessInput(inputSenderController.tickHistory[i].input);
                newTick.state = projectedState;
                newTick.input = inputSenderController.tickHistory[i].input;
                newTick.tick = inputSenderController.tickHistory[i].tick;
                inputSenderController.tickHistory[i] = newTick;
            }
    }

    public void ProcessServerRequests(ServerEventRequest[] requests)
    {
        for(int i=0; i<requests.Length; i++)
        {
            if (requests[i].requestInstances != null)
            {
                for (int j = 0; j < requests[i].requestInstances.Count; j++)
                {
                    liveController.ProcessServerEvents(i, requests[i].requestInstances[j]);
                }
            }
        }
    }
}
