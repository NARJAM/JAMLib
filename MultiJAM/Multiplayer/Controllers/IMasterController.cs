using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMasterController<PSM,IM,PIM> : MonoBehaviour
{
    public IPlayerController<PSM, IM, PIM> liveController;
    public IPlayerController<PSM, IM, PIM> projectionController;
    public IInputController<IM> inputController;
    public InputReceiverController<PSM,IM,PIM> inputReceiverController;
    public InputSenderController<PSM,IM,PIM> inputSenderController;
    public IPlayerView<PSM, PIM> mirrorPlayer;
    public PSM ghostState;
    public PSM projectedState;
    public PSM pastState;
    public TransportController<PIM> signalRController;

    public GameObject liveControllerObj;
    public GameObject projectionControllerObj;
    public GameObject inputControllerObj;
    public GameObject mirrorPlayerObj;

    public bool isOwner;
    public string connectionId;

    public abstract PSM CorrectPlayerState(PSM currentState,PSM serverState);
    public abstract bool CheckForCorrection(PSM serverState, PSM localState);

    public void Initialize(TransportController<PIM> _signalRController,PlayerStatePack<PSM,PIM> psp, bool _isOwner)
    {
        signalRController = _signalRController;
        liveController = liveControllerObj.GetComponent<IPlayerController<PSM, IM, PIM>>();
        projectionController = projectionControllerObj.GetComponent<IPlayerController<PSM,IM, PIM>> ();
        inputController = inputControllerObj.GetComponent<IInputController<IM>>();
        mirrorPlayer = mirrorPlayerObj.GetComponent<IPlayerView<PSM, PIM>>();
        liveController.initPlayer = psp.playerInit;
        isOwner = _isOwner;
        connectionId = psp.conId;
        liveController.Initialize(psp.playerState, _isOwner, connectionId, this);

        if (isOwner)
        {
            inputSenderController = new InputSenderController<PSM, IM, PIM>(this);
            inputSenderController.StartStream(connectionId);
            inputController.enabled = true;
        }
        else
        {
            inputReceiverController = new InputReceiverController<PSM, IM, PIM>(this);
            inputReceiverController.StartReception(connectionId);
            inputController.enabled = false;
            projectionController.gameObject.SetActive(false);
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

            if (CheckForCorrection(psp.playerState, pastTick.state))
            {
                ProjectState(psp);
            }
        }
    }

    public abstract void OnMirrorStateSet(PSM playerState);
    public abstract void OnGhostStateSet(PSM playerState);

    public void ProjectState(PlayerStatePack<PSM, PIM> psp)
    {
        TickModel<PSM, IM> pastTick = new TickModel<PSM, IM>();
        if (inputSenderController.tickHistory.TryGetValue(psp.tick, out pastTick))
        {
            projectionController.SetState(psp.playerState);
            pastState = psp.playerState;
            for (int i = psp.tick+1; i < inputSenderController.tickTrack; i++)
            {
                TickModel<PSM, IM> newTick = new TickModel<PSM, IM>();
                projectedState = projectionController.ProcessInput(inputSenderController.tickHistory[i].input);
                newTick.state = CorrectPlayerState(inputSenderController.tickHistory[i].state, projectedState);
                newTick.input = inputSenderController.tickHistory[i].input;
                newTick.tick = inputSenderController.tickHistory[i].tick;
                inputSenderController.tickHistory[i] = newTick;
            }

            TickModel<PSM, IM> newTick2 = new TickModel<PSM, IM>();
            projectedState = projectionController.ProcessInput(inputSenderController.tickHistory[inputSenderController.tickTrack].input);
            newTick2.state = CorrectPlayerState(inputSenderController.tickHistory[inputSenderController.tickTrack].state, projectedState);
            newTick2.input = inputSenderController.tickHistory[inputSenderController.tickTrack].input;
            newTick2.tick = inputSenderController.tickHistory[inputSenderController.tickTrack].tick;
            inputSenderController.tickHistory[inputSenderController.tickTrack] = newTick2;
            liveController.currentPlayerState = newTick2.state;
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
