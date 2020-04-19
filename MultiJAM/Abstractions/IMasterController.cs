﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMasterController : MonoBehaviour
{
    public IPlayerController liveController;
    public IInputController inputController;
    public InputReceiverController inputReceiverController;
    public InputSenderController inputSenderController;
    public IPlayerView mirrorPlayer;
    public PlayerStateModel ghostState;
    public PlayerStateModel projectedState;
    public PlayerStateModel pastState;
    public ITransportController transportController;

    public GameObject liveControllerObj;
    public GameObject inputControllerObj;
    public GameObject mirrorPlayerObj;

    public bool isOwner;
    public string connectionId;

    public abstract bool CheckForCorrection(PlayerStateModel serverState, PlayerStateModel localState, int tick);

    public void Initialize(ITransportController _transportController,PlayerStatePack psp, bool _isOwner)
    {
        transportController = _transportController;
        liveController = liveControllerObj.GetComponent<IPlayerController>();
        inputController = inputControllerObj.GetComponent<IInputController>();
        mirrorPlayer = mirrorPlayerObj.GetComponent<IPlayerView>();
        liveController.initPlayer = psp.playerInit;
        isOwner = _isOwner;
        connectionId = psp.conId;
        mirrorPlayer.isOwner = _isOwner;
        liveController.Initialize(psp.playerState, _isOwner, connectionId, this);

        if (isOwner)
        {
            inputSenderController = new InputSenderController(this);
            inputSenderController.StartStream(connectionId);
            inputController.enabled = true;
        }
        else
        {
            inputReceiverController = new InputReceiverController(this);
            inputReceiverController.InitStreamReception(connectionId);
            inputController.enabled = false;
        }
    }

    public void SetMirrorState(PlayerStateModel psp)
    {
        mirrorPlayer.SetFromModel(psp);
        OnMirrorStateSet(psp);
    }

    public void SetPlayerInit(PlayerInitModel pim) 
    {
        mirrorPlayer.SetInit(pim);
    }

    public void SetGhostState(PlayerStatePack psp)
    {
        ghostState = psp.playerState;
        OnGhostStateSet(psp.playerState);
        TickModel pastTick = new TickModel();

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

    public abstract void OnMirrorStateSet(PlayerStateModel playerState);
    public abstract void OnGhostStateSet(PlayerStateModel playerState);
    public abstract void OnPastStateSet(PlayerStateModel playerState);

    public void ProjectState(PlayerStatePack psp)
    {
        liveController.SetState(psp.playerState);
        for (int i = psp.tick; i < inputSenderController.tickTrack; i++)
            {
                TickModel newTick = new TickModel();
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
            if (requests[i].requestMessages != null)
            {
                for (int j = 0; j < requests[i].requestMessages.Length; j++)
                {
                    liveController.ProcessServerEvents(i, requests[i].requestMessages[j]);
                }
            }
        }
    }
}
