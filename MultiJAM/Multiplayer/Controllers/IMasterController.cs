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
    public IPlayerView<PSM, PIM> ghostPlayer;
    public IPlayerView<PSM, PIM> mirrorPlayer;
    public IPlayerView<PSM, PIM> projectedPlayer;
    public IPlayerView<PSM, PIM> pastPlayer;
    public TransportController<PIM> signalRController;

    public GameObject liveControllerObj;
    public GameObject projectionControllerObj;
    public GameObject inputControllerObj;
    public GameObject ghostPlayerObj;
    public GameObject mirrorPlayerObj;
    public GameObject projectedPlayerObj;
    public GameObject pastPlayerObj;

    public bool isOwner;
    public string connectionId;

    public abstract void CorrectPlayerState(PSM serverState);
    public abstract bool CheckForCorrection(PSM serverState, PSM localState);

    public void Initialize(TransportController<PIM> _signalRController,PlayerStatePack<PSM,PIM> psp, bool _isOwner)
    {
        signalRController = _signalRController;
        liveController = liveControllerObj.GetComponent<IPlayerController<PSM, IM, PIM>>();
        projectionController = projectionControllerObj.GetComponent<IPlayerController<PSM,IM, PIM>> ();
        inputController = inputControllerObj.GetComponent<IInputController<IM>>();
        ghostPlayer = ghostPlayerObj.GetComponent<IPlayerView<PSM, PIM>>();
        mirrorPlayer = mirrorPlayerObj.GetComponent<IPlayerView<PSM, PIM>>();
        projectedPlayer = projectedPlayerObj.GetComponent<IPlayerView<PSM, PIM>>();
        pastPlayer = pastPlayerObj.GetComponent<IPlayerView<PSM, PIM>>();
        liveController.initPlayer = psp.playerInit;
        isOwner = _isOwner;
        connectionId = psp.conId;
        mirrorPlayer.isOwner = _isOwner;
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
            pastPlayer.gameObject.SetActive(false);
            ghostPlayer.gameObject.SetActive(false);
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
        ghostPlayer.SetFromModel(psp.playerState);
        OnGhostStateSet(psp.playerState);
        TickModel<PSM, IM> pastTick = new TickModel<PSM, IM>();

        if (inputSenderController.tickHistory.TryGetValue(psp.tick, out pastTick))
        {
            pastPlayer.SetFromModel(pastTick.state);

            if (CheckForCorrection(psp.playerState, pastTick.state))
            {
                PSM projectedState = ProjectState(psp);
                projectedPlayer.SetFromModel(projectedState);
                CorrectPlayerState(projectedState);
            }
        }
    }

    public abstract void OnMirrorStateSet(PSM playerState);
    public abstract void OnGhostStateSet(PSM playerState);

    public PSM ProjectState(PlayerStatePack<PSM, PIM> psp)
    {
        TickModel<PSM, IM> pastTick = new TickModel<PSM, IM>();
        if (inputSenderController.tickHistory.TryGetValue(psp.tick, out pastTick))
        {
            projectionController.SetState(psp.playerState);
            pastPlayer.SetFromModel(psp.playerState);
            for (int i = psp.tick+1; i < inputSenderController.tickTrack; i++)
            {
                TickModel<PSM, IM> newTick = new TickModel<PSM, IM>();
                newTick.state = projectionController.ProcessInput(inputSenderController.tickHistory[i].input);
                newTick.input = inputSenderController.tickHistory[i].input;
                newTick.tick = inputSenderController.tickHistory[i].tick;
                inputSenderController.tickHistory[i] = newTick;
            }
        }
        return projectionController.currentPlayerState;
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
